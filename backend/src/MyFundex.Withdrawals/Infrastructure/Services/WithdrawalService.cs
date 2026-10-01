using Microsoft.EntityFrameworkCore;
using MyFundex.BuildingBlocks.Abstractions;
using MyFundex.Contracts;

namespace MyFundex.Withdrawals;

public sealed class WithdrawalService(
    WithdrawalDbContext db,
    IFundedAccountReader accounts,
    IWithdrawalFunds funds,
    IPayoutGateway payouts,
    ICurrentActor actor
)
{
    public async Task<WithdrawalRequest> RequestAsync(
        Guid accountId,
        Guid requestId,
        decimal amount,
        CancellationToken ct
    )
    {
        var account = await accounts.GetByPublicIdAsync(accountId, ct);
        if (
            account == null
            || actor.ActorId <= 0
            || account.UserId != actor.ActorId
            || account.TradingMode != "Funded"
            || account.Status != "Active"
        )
            throw new ArgumentException("An active real account is required.");
        if (requestId == Guid.Empty || amount < 1 || decimal.Round(amount, 2) != amount)
            throw new ArgumentException("Invalid withdrawal amount or reference.");
        var request = await db.Requests.SingleOrDefaultAsync(x => x.WithdrawalId == requestId, ct);
        if (request == null)
        {
            request = new WithdrawalRequest
            {
                WithdrawalId = requestId,
                FundedAccountInternalId = account.InternalId,
                UserInternalId = actor.ActorId,
                RequestedAmount = amount,
                EligibleAmount = amount,
                NetPayoutAmount = amount,
                Status = "Reserving",
            };
            db.Add(request);
            await db.SaveChangesAsync(ct);
        }
        if (
            request.FundedAccountInternalId != account.InternalId
            || request.RequestedAmount != amount
        )
            throw new ArgumentException("Withdrawal reference mismatch.");
        if (request.Status == "Reserving")
        {
            await funds.ReserveAsync(request.WithdrawalId, account.InternalId, amount, ct);
            request.Status = "Requested";
            await db.SaveChangesAsync(ct);
        }
        return request;
    }

    public async Task ReviewAsync(Guid id, bool approve, string? beneficiary, CancellationToken ct)
    {
        var request = await db.Requests.SingleAsync(x => x.WithdrawalId == id, ct);
        if (request.Status != "Requested")
            throw new ArgumentException("Withdrawal is not awaiting review.");
        if (
            approve
            && (
                string.IsNullOrWhiteSpace(beneficiary)
                || !beneficiary.StartsWith("fa_", StringComparison.Ordinal)
                || beneficiary.Length > 100
            )
        )
            throw new ArgumentException("A verified Razorpay beneficiary reference is required.");
        request.BeneficiaryReference = approve ? beneficiary : null;
        request.Status = approve ? "Approved" : "Rejecting";
        await db.SaveChangesAsync(ct);
        if (!approve)
        {
            await funds.CompleteAsync(id, false, ct);
            request.Status = "Rejected";
            await db.SaveChangesAsync(ct);
        }
    }

    public async Task ReconcileProviderIdAsync(Guid id, string payoutId, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(payoutId) || payoutId.Length > 100)
            throw new ArgumentException("A payout identifier is required.");
        var request = await db.Requests.SingleAsync(x => x.WithdrawalId == id, ct);
        if (
            request.Status != "Processing"
            || request.ProviderPayoutId != null && request.ProviderPayoutId != payoutId
        )
            throw new ArgumentException("Only the original uncertain payout can be reconciled.");
        var result = await payouts.ReadAsync(payoutId, ct);
        if (
            result.Id != payoutId
            || result.ReferenceId != id.ToString("N")
            || result.Amount != request.NetPayoutAmount
        )
            throw new ArgumentException("Provider payout does not match this withdrawal.");
        request.ProviderPayoutId = result.Id;
        request.NextCheckAt = null;
        await db.SaveChangesAsync(ct);
    }

    public async Task ProcessAsync(Guid id, CancellationToken ct)
    {
        var request = await db.Requests.SingleAsync(x => x.WithdrawalId == id, ct);
        if (request.Status == "Rejecting")
        {
            await funds.CompleteAsync(id, false, ct);
            request.Status = "Rejected";
            await db.SaveChangesAsync(ct);
            return;
        }
        if (request.Status is not ("Approved" or "Processing" or "Paid"))
            return;
        if (request.Status == "Approved")
        {
            request.Status = "Processing";
            request.PayoutStartedAt = DateTimeOffset.UtcNow;
            await db.SaveChangesAsync(ct);
        }
        if (
            request.ProviderPayoutId == null
            && request.PayoutStartedAt < DateTimeOffset.UtcNow.AddMinutes(-15)
        )
            throw new ArgumentException(
                "Uncertain payout needs provider reconciliation; automatic submission retries have stopped."
            );
        // Claim the next poll before contacting the provider; competing workers lose the version check.
        request.NextCheckAt = DateTimeOffset.UtcNow.Add(
            request.Status == "Paid" ? TimeSpan.FromMinutes(15) : TimeSpan.FromSeconds(30)
        );
        await db.SaveChangesAsync(ct);
        var result =
            request.ProviderPayoutId == null
                ? await payouts.SendAsync(
                    id,
                    request.BeneficiaryReference!,
                    request.NetPayoutAmount,
                    ct
                )
                : await payouts.ReadAsync(request.ProviderPayoutId, ct);
        if (result.Amount != request.NetPayoutAmount || result.ReferenceId != id.ToString("N"))
            throw new ArgumentException("Payout identity mismatch.");
        request.ProviderPayoutId = result.Id;
        await db.SaveChangesAsync(ct);
        if (result.Status == "processed")
        {
            await funds.CompleteAsync(id, true, ct);
            request.Status = "Paid";
        }
        else if (result.Status is "failed" or "reversed" or "rejected" or "cancelled")
        {
            await funds.CompleteAsync(id, false, ct);
            request.Status = "Failed";
        }
        await db.SaveChangesAsync(ct);
    }
}
