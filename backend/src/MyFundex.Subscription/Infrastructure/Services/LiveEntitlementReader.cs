using Microsoft.EntityFrameworkCore;
using MyFundex.Contracts;

namespace MyFundex.Subscription;

public sealed class LiveEntitlementReader(SubscriptionDbContext db) : ILiveEntitlementReader
{
    public async Task<LiveEntitlement?> GetAsync(Guid subscriptionId, CancellationToken ct)
    {
        var subscription = await db
            .Subscriptions.AsNoTracking()
            .SingleOrDefaultAsync(
                x =>
                    x.SubscriptionId == subscriptionId
                    && x.Status == "Completed"
                    && x.ActivatedAt != null
                    && x.CompletedAt != null,
                ct
            );
        if (subscription == null)
            return null;
        var version = await db
            .PlanVersions.AsNoTracking()
            .SingleAsync(x => x.Id == subscription.PlanVersionInternalId, ct);
        var stages = await db
            .Stages.AsNoTracking()
            .Where(x => x.PlanVersionInternalId == version.Id)
            .OrderBy(x => x.StageNumber)
            .ToListAsync(ct);
        var passed = await db.Set<ChallengeAttempt>()
            .AsNoTracking()
            .Where(x =>
                x.SubscriptionInternalId == subscription.Id
                && x.Status == "Passed"
                && x.CompletedAt != null
            )
            .Select(x => x.StageInternalId)
            .ToListAsync(ct);
        if (stages.Count is not (2 or 3) || stages.Any(x => !passed.Contains(x.Id)))
            return null;
        return new(
            subscriptionId,
            subscription.Id,
            subscription.UserInternalId,
            version.ChallengeCapital,
            stages[^1].PolicySetId,
            version.RewardSharePercent,
            version.FundedDailyLossPercent,
            version.FundedTotalLossPercent,
            version.TaxWithholdingPercent,
            version.OtherDeductionPercent
        );
    }
}

public sealed class LiveRiskTermsReader(
    SubscriptionDbContext db,
    IFundedAccountReader accounts,
    ILiveEntitlementReader entitlements
) : ILiveRiskTermsReader
{
    public async Task<LiveEntitlement?> GetByAccountAsync(Guid accountId, CancellationToken ct)
    {
        var account = await accounts.GetByPublicIdAsync(accountId, ct);
        if (account == null || account.TradingMode != "Funded")
            return null;
        var id = await db
            .Subscriptions.AsNoTracking()
            .Where(x => x.Id == account.SubscriptionInternalId)
            .Select(x => (Guid?)x.SubscriptionId)
            .SingleOrDefaultAsync(ct);
        return id == null ? null : await entitlements.GetAsync(id.Value, ct);
    }
}
