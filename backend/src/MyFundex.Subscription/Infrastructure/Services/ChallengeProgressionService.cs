using Microsoft.EntityFrameworkCore;
using MyFundex.Contracts;

namespace MyFundex.Subscription;

public sealed class EvaluationTermsReader(SubscriptionDbContext db) : IEvaluationTermsReader
{
    public Task<EvaluationTerms?> GetAsync(Guid accountId, CancellationToken ct) =>
        (
            from attempt in db.Set<ChallengeAttempt>().AsNoTracking()
            join stage in db.Stages.AsNoTracking() on attempt.StageInternalId equals stage.Id
            where attempt.AccountId == accountId
            select new EvaluationTerms(
                stage.StartingCapital,
                stage.ProfitTargetPercent,
                stage.MaxDailyLossPercent,
                stage.MaxTotalLossPercent,
                stage.MinimumTradingDays,
                stage.MaximumCalendarDays,
                attempt.StartedAt
            )
        ).SingleOrDefaultAsync(ct);
}

public sealed class ChallengeProgressionService(
    SubscriptionDbContext db,
    IEvaluationProgressReader progress,
    IEvaluationAccountProvisioner provisioner,
    IAccountLifecycle accounts
)
{
    public async Task AdvanceAsync(Guid attemptId, CancellationToken ct)
    {
        var attempt = await db.Set<ChallengeAttempt>()
            .SingleAsync(x => x.AttemptId == attemptId, ct);
        if (
            attempt.AccountId == null
            || attempt.Status is not ("InProgress" or "Passed" or "Failed")
        )
            return;
        var subscription = await db.Subscriptions.SingleAsync(
            x => x.Id == attempt.SubscriptionInternalId,
            ct
        );
        if (subscription.Status is "Completed" or "Failed")
            return;
        var state = await progress.GetAsync(attempt.AccountId.Value, ct);
        if (state == null || state.Status == "Active")
            return;
        if (state.Status is not ("Passed" or "Failed"))
            return;
        // A durable terminal paper-book state blocks further trading before cross-module progression.
        await accounts.CloseEvaluationAsync(attempt.AccountId.Value, state.Status, ct);
        attempt.Status = state.Status;
        attempt.CompletedAt ??= DateTimeOffset.UtcNow;
        if (state.Status == "Failed")
        {
            subscription.Status = "Failed";
            db.Add(
                new ChallengeNotice
                {
                    UserInternalId = subscription.UserInternalId,
                    SubscriptionInternalId = subscription.Id,
                    Outcome = "Failed",
                }
            );
            subscription.CompletedAt = DateTimeOffset.UtcNow;
            await db.SaveChangesAsync(ct);
            return;
        }
        var stage = await db
            .Stages.AsNoTracking()
            .SingleAsync(x => x.Id == attempt.StageInternalId, ct);
        var nextStage = await db
            .Stages.AsNoTracking()
            .Where(x =>
                x.PlanVersionInternalId == stage.PlanVersionInternalId
                && x.StageNumber > stage.StageNumber
            )
            .OrderBy(x => x.StageNumber)
            .FirstOrDefaultAsync(ct);
        if (nextStage == null)
        {
            subscription.Status = "Completed";
            db.Add(
                new ChallengeNotice
                {
                    UserInternalId = subscription.UserInternalId,
                    SubscriptionInternalId = subscription.Id,
                    Outcome = "Passed",
                }
            );
            subscription.CompletedAt = DateTimeOffset.UtcNow;
            await db.SaveChangesAsync(ct);
            return;
        }
        var next = await db.Set<ChallengeAttempt>()
            .SingleOrDefaultAsync(
                x =>
                    x.SubscriptionInternalId == subscription.Id
                    && x.StageInternalId == nextStage.Id,
                ct
            );
        if (next == null)
        {
            next = new ChallengeAttempt
            {
                AttemptId = Guid.NewGuid(),
                SubscriptionInternalId = subscription.Id,
                StageInternalId = nextStage.Id,
                StartedAt = DateTimeOffset.UtcNow,
            };
            db.Add(next);
            await db.SaveChangesAsync(ct);
        }
        next.AccountId = await provisioner.ProvisionAsync(
            new(
                next.AttemptId,
                subscription.UserInternalId,
                subscription.Id,
                nextStage.StartingCapital,
                nextStage.PolicySetId
            ),
            ct
        );
        next.Status = "InProgress";
        await db.SaveChangesAsync(ct);
    }
}
