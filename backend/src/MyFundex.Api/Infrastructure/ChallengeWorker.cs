using Microsoft.EntityFrameworkCore;
using MyFundex.Subscription;

namespace MyFundex.Api.Infrastructure;

public sealed class ChallengeWorker(IServiceScopeFactory scopes, ILogger<ChallengeWorker> logger)
    : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(10));
        long after = 0;
        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            try
            {
                using var readScope = scopes.CreateScope();
                var db = readScope.ServiceProvider.GetRequiredService<SubscriptionDbContext>();
                var attempts = await (
                    from attempt in db.Set<ChallengeAttempt>().AsNoTracking()
                    join subscription in db.Subscriptions.AsNoTracking()
                        on attempt.SubscriptionInternalId equals subscription.Id
                    where
                        subscription.Status == "Evaluation"
                        && attempt.AccountId != null
                        && attempt.Id > after
                    orderby attempt.Id
                    select new { attempt.Id, attempt.AttemptId }
                )
                    .Take(100)
                    .ToListAsync(stoppingToken);
                foreach (var attempt in attempts)
                {
                    try
                    {
                        using var scope = scopes.CreateScope();
                        await scope
                            .ServiceProvider.GetRequiredService<ChallengeProgressionService>()
                            .AdvanceAsync(attempt.AttemptId, stoppingToken);
                    }
                    catch (Exception error) when (error is not OperationCanceledException)
                    {
                        logger.LogError(
                            error,
                            "Challenge {AttemptId} will be retried",
                            attempt.AttemptId
                        );
                    }
                }
                after = attempts.Count == 100 ? attempts[^1].Id : 0;
            }
            catch (Exception error) when (error is not OperationCanceledException)
            {
                logger.LogError(error, "Challenge monitoring will be retried");
            }
        }
    }
}
