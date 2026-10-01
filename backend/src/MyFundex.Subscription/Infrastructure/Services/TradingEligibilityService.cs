using Microsoft.EntityFrameworkCore;
using MyFundex.Contracts;

namespace MyFundex.Subscription;

public sealed class TradingEligibilityService(
    SubscriptionDbContext db,
    IFundedAccountReader? accounts = null,
    ILiveEntitlementReader? entitlements = null
) : ITradingEligibility
{
    public async Task<TradingEligibility> CheckAsync(
        Guid accountId,
        long userId,
        string mode,
        CancellationToken ct
    )
    {
        if (mode == "Funded" && accounts != null && entitlements != null)
        {
            var account = await accounts.GetByPublicIdAsync(accountId, ct);
            if (
                account == null
                || account.UserId != userId
                || account.TradingMode != "Funded"
                || account.Status != "Active"
            )
                return new(false, "Live account is not active.");
            var subscription = await db
                .Subscriptions.AsNoTracking()
                .SingleOrDefaultAsync(
                    x => x.Id == account.SubscriptionInternalId && x.UserInternalId == userId,
                    ct
                );
            var entitlement =
                subscription == null
                    ? null
                    : await entitlements.GetAsync(subscription.SubscriptionId, ct);
            return entitlement == null ? new(false, "Verified graduation is required.") : new(true);
        }
        if (mode != "Evaluation")
            return new(false, "Verified graduation is required.");
        var eligible = await (
            from attempt in db.Set<ChallengeAttempt>().AsNoTracking()
            join subscription in db.Subscriptions.AsNoTracking()
                on attempt.SubscriptionInternalId equals subscription.Id
            where
                attempt.AccountId == accountId
                && attempt.Status == "InProgress"
                && subscription.UserInternalId == userId
                && subscription.Status == "Evaluation"
                && subscription.ActivatedAt != null
            select attempt.Id
        ).AnyAsync(ct);
        return eligible
            ? new(true)
            : new(false, "An active paid evaluation is required for this account.");
    }
}
