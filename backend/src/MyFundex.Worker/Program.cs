using Microsoft.EntityFrameworkCore;
using MyFundex.BuildingBlocks.Abstractions;
using MyFundex.MarketData;
using MyFundex.Worker;
var b=Host.CreateApplicationBuilder(args);var cs=b.Configuration.GetConnectionString("Postgres")!;b.Services.AddSingleton<ICurrentActor,SystemActor>();b.Services.AddMarketDataModule(cs);b.Services.AddHttpClient<UpstoxInstrumentSyncService>();b.Services.AddHostedService<InstrumentSyncWorker>();await b.Build().RunAsync();
namespace MyFundex.Worker{public sealed class SystemActor:ICurrentActor{public long ActorId=>1;}}
