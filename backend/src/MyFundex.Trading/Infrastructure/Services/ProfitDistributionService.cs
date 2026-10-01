using Microsoft.EntityFrameworkCore;
using MyFundex.Contracts;

namespace MyFundex.Trading;

public sealed class ProfitDistributionService(
    TradingDbContext db,
    IFundedAccountReader accounts,
    ILiveEntitlementReader entitlements,
    IWalletLedger ledger
)
{
    public async Task<ProfitDistribution> SettleAsync(
        Guid accountId,
        Guid subscriptionId,
        Guid requestId,
        decimal fees,
        string reference,
        CancellationToken ct
    )
    {
        if (
            requestId == Guid.Empty
            || fees < 0
            || decimal.Round(fees, 2) != fees
            || string.IsNullOrWhiteSpace(reference)
            || reference.Length > 150
        )
            throw new ArgumentException(
                "A verified broker settlement reference and nonnegative charges are required."
            );
        var existing = await db.Set<ProfitDistribution>()
            .SingleOrDefaultAsync(x => x.DistributionId == requestId, ct);
        if (existing != null)
        {
            if (
                existing.AccountId != accountId
                || existing.Fees != fees
                || existing.SettlementReference != reference
            )
                throw new ArgumentException(
                    "Settlement reference was used for a different request."
                );
            return existing;
        }
        var account = await accounts.GetByPublicIdAsync(accountId, ct);
        var entitlement = await entitlements.GetAsync(subscriptionId, ct);
        if (
            account == null
            || account.TradingMode != "Funded"
            || account.Status != "Active"
            || entitlement == null
            || account.SubscriptionInternalId != entitlement.SubscriptionInternalId
        )
            throw new ArgumentException("A verified active funded account is required.");
        var book = await db.Set<LiveBook>().SingleAsync(x => x.AccountId == accountId, ct);
        if (
            book.Status != "Active"
            || book.ReservedCash != 0
            || await db.Set<LivePosition>()
                .AnyAsync(
                    x => x.AccountId == accountId && (x.Quantity != 0 || x.ReservedQuantity != 0),
                    ct
                )
            || await db.Orders.AnyAsync(
                x =>
                    x.AccountId == accountId
                    && x.Status != "Filled"
                    && x.Status != "Cancelled"
                    && x.Status != "Rejected",
                ct
            )
        )
            throw new ArgumentException(
                "Resolve all orders and positions before settling rewards."
            );
        var surplus = decimal.Floor((book.Cash - account.FundedCapital) * 100m) / 100m;
        if (surplus <= fees)
            throw new ArgumentException("There is no net settled profit to distribute.");
        var net = surplus - fees;
        var reward = decimal.Floor(net * entitlement.RewardSharePercent) / 100m;
        if (reward <= 0 || reward > net)
            throw new ArgumentException("Invalid plan reward share.");
        var distribution = new ProfitDistribution
        {
            DistributionId = requestId,
            AccountId = accountId,
            AccountInternalId = account.InternalId,
            SettlementReference = reference,
            GrossProfit = surplus,
            Fees = fees,
            TraderReward = reward,
            PlatformReward = net - reward,
        };
        // Remove the entire settled surplus before delivering the trader share. It cannot also fund orders.
        book.Cash -= surplus;
        book.Equity = book.Cash;
        book.DayOpeningEquity = Math.Max(account.FundedCapital, book.DayOpeningEquity - surplus);
        db.Add(distribution);
        await db.SaveChangesAsync(ct);
        return distribution;
    }

    public async Task DeliverAsync(Guid id, CancellationToken ct)
    {
        var item = await db.Set<ProfitDistribution>().SingleAsync(x => x.DistributionId == id, ct);
        if (item.DeliveredAt != null)
            return;
        await ledger.CreditProfitAsync(
            item.AccountInternalId,
            item.TraderReward,
            "VerifiedSettlement",
            id.ToString("N"),
            ct
        );
        item.DeliveredAt = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(ct);
    }
}
