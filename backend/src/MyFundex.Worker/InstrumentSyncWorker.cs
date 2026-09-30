using System.IO.Compression;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using MyFundex.MarketData;

namespace MyFundex.Worker;

public sealed class InstrumentSyncWorker(
    IServiceScopeFactory scopes,
    ILogger<InstrumentSyncWorker> log
) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var s = scopes.CreateScope();
                await s
                    .ServiceProvider.GetRequiredService<UpstoxInstrumentSyncService>()
                    .SyncAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                log.LogError(ex, "Upstox instrument sync failed");
            }
            await Task.Delay(TimeSpan.FromHours(12), stoppingToken);
        }
    }
}
