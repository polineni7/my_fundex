using Microsoft.EntityFrameworkCore;
using MyFundex.Contracts;

namespace MyFundex.Subscription;

public sealed class TradingEligibilityService(SubscriptionDbContext db) : ITradingEligibility
{
    public async Task<TradingEligibility> CheckAsync(
        Guid accountId,
        long userId,
        string mode,
        CancellationToken ct
    )
    {
        if (mode != "Evaluation")
            return new(
                false,
                "Live account graduation and allocation must be verified before trading."
            );
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
