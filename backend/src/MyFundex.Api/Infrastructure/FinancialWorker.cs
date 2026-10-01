using Microsoft.EntityFrameworkCore;
using MyFundex.Trading;
using MyFundex.Withdrawals;

namespace MyFundex.Api.Infrastructure;

public sealed class FinancialWorker(IServiceScopeFactory scopes, ILogger<FinancialWorker> logger)
    : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(10));
        long orderCursor = 0,
            payoutCursor = 0,
            accountCursor = 0,
            paperCursor = 0;
        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            try
            {
                using var read = scopes.CreateScope();
                var trading = read.ServiceProvider.GetRequiredService<TradingDbContext>();
                var paperOrders = await trading
                    .Orders.AsNoTracking()
                    .Where(x =>
                        x.BrokerProvider == "Paper" && x.Status == "Open" && x.Id > paperCursor
                    )
                    .OrderBy(x => x.Id)
                    .Take(100)
                    .Select(x => new { x.Id, x.OrderId })
                    .ToListAsync(stoppingToken);
                foreach (var order in paperOrders)
                    await RunAsync(
                        order.OrderId,
                        async services =>
                            await services
                                .GetRequiredService<PaperTradingEngine>()
                                .MatchAsync(order.OrderId, stoppingToken)
                    );
                paperCursor = paperOrders.Count == 100 ? paperOrders[^1].Id : 0;
                var orders = await trading
                    .Orders.AsNoTracking()
                    .Where(x =>
                        x.UsesLiveBook
                        && x.Id > orderCursor
                        && x.Status != "Filled"
                        && x.Status != "Cancelled"
                        && x.Status != "Rejected"
                    )
                    .OrderBy(x => x.Id)
                    .Take(100)
                    .Select(x => new { x.Id, x.OrderId })
                    .ToListAsync(stoppingToken);
                foreach (var order in orders)
                    await RunAsync(
                        order.OrderId,
                        async services =>
                            await services
                                .GetRequiredService<LiveSettlementService>()
                                .ReconcileAsync(order.OrderId, stoppingToken)
                    );
                orderCursor = orders.Count == 100 ? orders[^1].Id : 0;
                var books = await trading
                    .Set<LiveBook>()
                    .AsNoTracking()
                    .Where(x => x.Id > accountCursor && x.Status != "Closed")
                    .OrderBy(x => x.Id)
                    .Take(100)
                    .Select(x => new { x.Id, x.AccountId })
                    .ToListAsync(stoppingToken);
                foreach (var book in books)
                {
                    await RunAsync(
                        book.AccountId,
                        async services =>
                            await services
                                .GetRequiredService<LiveRiskMonitor>()
                                .CheckAsync(book.AccountId, stoppingToken)
                    );
                    await RunAsync(
                        book.AccountId,
                        async services =>
                            await services
                                .GetRequiredService<SquareOffService>()
                                .ProcessAsync(book.AccountId, stoppingToken)
                    );
                }
                accountCursor = books.Count == 100 ? books[^1].Id : 0;
                var withdrawals = read.ServiceProvider.GetRequiredService<WithdrawalDbContext>();
                var requests = await withdrawals
                    .Requests.AsNoTracking()
                    .Where(x =>
                        x.Id > payoutCursor
                        && (x.NextCheckAt == null || x.NextCheckAt <= DateTimeOffset.UtcNow)
                        && (
                            x.Status == "Approved"
                            || x.Status == "Processing"
                            || x.Status == "Rejecting"
                            || x.Status == "Paid"
                        )
                    )
                    .OrderBy(x => x.Id)
                    .Take(100)
                    .Select(x => new { x.Id, x.WithdrawalId })
                    .ToListAsync(stoppingToken);
                foreach (var request in requests)
                    await RunAsync(
                        request.WithdrawalId,
                        async services =>
                            await services
                                .GetRequiredService<WithdrawalService>()
                                .ProcessAsync(request.WithdrawalId, stoppingToken)
                    );
                payoutCursor = requests.Count == 100 ? requests[^1].Id : 0;
                var distributions = await trading
                    .Set<ProfitDistribution>()
                    .AsNoTracking()
                    .Where(x => x.DeliveredAt == null)
                    .OrderBy(x => x.Id)
                    .Take(100)
                    .Select(x => x.DistributionId)
                    .ToListAsync(stoppingToken);
                foreach (var id in distributions)
                    await RunAsync(
                        id,
                        async services =>
                            await services
                                .GetRequiredService<ProfitDistributionService>()
                                .DeliverAsync(id, stoppingToken)
                    );
            }
            catch (Exception error) when (error is not OperationCanceledException)
            {
                logger.LogError(error, "Financial monitoring will retry");
            }
        }
    }

    private async Task RunAsync(Guid id, Func<IServiceProvider, Task> action)
    {
        try
        {
            using var scope = scopes.CreateScope();
            await action(scope.ServiceProvider);
        }
        catch (Exception error) when (error is not OperationCanceledException)
        {
            logger.LogError(
                error,
                "Financial operation {OperationId} requires retry or review",
                id
            );
        }
    }
}
