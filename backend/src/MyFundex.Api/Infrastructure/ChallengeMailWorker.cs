using Microsoft.EntityFrameworkCore;
using MyFundex.Identity;
using MyFundex.Messaging.Contracts;
using MyFundex.Subscription;

namespace MyFundex.Api.Infrastructure;

public sealed class ChallengeMailWorker(
    IServiceScopeFactory scopes,
    ILogger<ChallengeMailWorker> logger
) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(30));
        while (await timer.WaitForNextTickAsync(ct))
        {
            try
            {
                using var read = scopes.CreateScope();
                var db = read.ServiceProvider.GetRequiredService<SubscriptionDbContext>();
                var ids = await db.Set<ChallengeNotice>()
                    .AsNoTracking()
                    .Where(x =>
                        x.DeliveredAt == null
                        && (x.LeaseUntil == null || x.LeaseUntil < DateTimeOffset.UtcNow)
                    )
                    .OrderBy(x => x.Id)
                    .Take(50)
                    .Select(x => x.NoticeId)
                    .ToListAsync(ct);
                foreach (var id in ids)
                {
                    try
                    {
                        using var scope = scopes.CreateScope();
                        var local =
                            scope.ServiceProvider.GetRequiredService<SubscriptionDbContext>();
                        var notice = await local
                            .Set<ChallengeNotice>()
                            .SingleAsync(x => x.NoticeId == id, ct);
                        if (notice.DeliveredAt != null || notice.LeaseUntil > DateTimeOffset.UtcNow)
                            continue;
                        notice.LeaseUntil = DateTimeOffset.UtcNow.AddMinutes(5);
                        notice.Attempts++;
                        await local.SaveChangesAsync(ct);
                        var identity =
                            scope.ServiceProvider.GetRequiredService<IdentityDbContext>();
                        var email = await identity
                            .Users.AsNoTracking()
                            .Where(x => x.Id == notice.UserInternalId)
                            .Select(x => x.Email)
                            .SingleAsync(ct);
                        var subject =
                            notice.Outcome == "Passed"
                                ? "Congratulations - evaluation completed"
                                : "Your evaluation result";
                        var message =
                            notice.Outcome == "Passed"
                                ? "You passed every evaluation stage. Your real account will become available after allocation approval and broker verification."
                                : "Your evaluation reached a loss or time limit. You can purchase a new attempt; your previous history remains available.";
                        await scope
                            .ServiceProvider.GetRequiredService<IEmailTransport>()
                            .SendAsync(new(email, subject, message), ct);
                        notice.DeliveredAt = DateTimeOffset.UtcNow;
                        notice.LeaseUntil = null;
                        await local.SaveChangesAsync(ct);
                    }
                    catch (Exception error) when (error is not OperationCanceledException)
                    {
                        logger.LogError(
                            error,
                            "Challenge notice {NoticeId} will retry after its lease expires",
                            id
                        );
                    }
                }
            }
            catch (Exception error) when (error is not OperationCanceledException)
            {
                logger.LogError(error, "Challenge mail polling failed");
            }
        }
    }
}
