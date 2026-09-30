using Microsoft.EntityFrameworkCore;
using MyFundex.Contracts;

namespace MyFundex.Subscription;

public sealed class PaidSubscriptionActivator(
    SubscriptionDbContext db,
    IEvaluationAccountProvisioner accounts
) : IPaidSubscriptionActivator
{
    public async Task ActivateAsync(Guid subscriptionId, long userId, CancellationToken ct)
    {
        var subscription = await db.Subscriptions.SingleAsync(
            x => x.SubscriptionId == subscriptionId && x.UserInternalId == userId,
            ct
        );
        if (subscription.Status != "PendingPayment")
            return;
        var stage = await db
            .Stages.AsNoTracking()
            .Where(x => x.PlanVersionInternalId == subscription.PlanVersionInternalId)
            .OrderBy(x => x.StageNumber)
            .FirstAsync(ct);
        var attempt = await db.Set<ChallengeAttempt>()
            .SingleOrDefaultAsync(
                x => x.SubscriptionInternalId == subscription.Id && x.StageInternalId == stage.Id,
                ct
            );
        if (attempt is null)
        {
            attempt = new ChallengeAttempt
            {
                AttemptId = Guid.NewGuid(),
                SubscriptionInternalId = subscription.Id,
                StageInternalId = stage.Id,
                StartedAt = DateTimeOffset.UtcNow,
            };
            db.Add(attempt);
            await db.SaveChangesAsync(ct);
        }
        var accountId = await accounts.ProvisionAsync(
            new(
                attempt.AttemptId,
                userId,
                subscription.Id,
                stage.StartingCapital,
                stage.PolicySetId
            ),
            ct
        );
        attempt.AccountId = accountId;
        attempt.Status = "InProgress";
        subscription.Status = "Evaluation";
        subscription.ActivatedAt = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(ct);
    }
}
