using Microsoft.EntityFrameworkCore;
using MyFundex.Contracts;
using MyFundex.Payments;

namespace MyFundex.Api.Infrastructure;

public sealed class PaymentFulfilmentWorker(
    IServiceScopeFactory scopes,
    ILogger<PaymentFulfilmentWorker> logger
) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var readScope = scopes.CreateScope();
                var reader = readScope.ServiceProvider.GetRequiredService<PaymentsDbContext>();
                var ids = await reader
                    .Payments.AsNoTracking()
                    .Where(x =>
                        x.Status == "Paid" && x.FulfilledAt == null && x.SubscriptionId != null
                    )
                    .OrderBy(x => x.PaidAt)
                    .Take(100)
                    .Select(x => x.PaymentId)
                    .ToListAsync(stoppingToken);
                foreach (var id in ids)
                {
                    try
                    {
                        using var scope = scopes.CreateScope();
                        var db = scope.ServiceProvider.GetRequiredService<PaymentsDbContext>();
                        var payment = await db.Payments.SingleAsync(
                            x => x.PaymentId == id,
                            stoppingToken
                        );
                        if (payment.FulfilledAt != null)
                            continue;
                        var activator =
                            scope.ServiceProvider.GetRequiredService<IPaidSubscriptionActivator>();
                        await activator.ActivateAsync(
                            payment.SubscriptionId!.Value,
                            payment.UserInternalId,
                            stoppingToken
                        );
                        payment.FulfilledAt = DateTimeOffset.UtcNow;
                        await db.SaveChangesAsync(stoppingToken);
                    }
                    catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                    {
                        throw;
                    }
                    catch (Exception exception)
                    {
                        logger.LogError(
                            exception,
                            "Payment {PaymentId} fulfilment will be retried.",
                            id
                        );
                    }
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "Payment fulfilment polling will be retried.");
            }
            await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
        }
    }
}
