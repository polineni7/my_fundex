using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MyFundex.BuildingBlocks.Abstractions;
using MyFundex.BuildingBlocks.Domain;
using MyFundex.BuildingBlocks.Persistence;
using MyFundex.Contracts;

namespace MyFundex.Broker;

public static class BrokerModule
{
    public static IServiceCollection AddBrokerModule(this IServiceCollection s, string cs)
    {
        s.AddScoped<BrokerConfigurationService>();
        s.AddScoped<IRuntimeSettingSource>(sp => sp.GetRequiredService<BrokerConfigurationService>());
        s.AddDbContext<BrokerDbContext>(o => o.UseNpgsql(cs));
        s.AddHttpClient<UpstoxOrderGateway>();
        s.AddHttpClient<IBrokerVerificationAdapter, UpstoxAccountVerifier>();
        s.AddHttpClient<IBrokerReconciliationAdapter, UpstoxOrderReader>();
        s.AddScoped<BrokerLifecycleRouter>();
        s.AddScoped<IBrokerAccountVerifier>(sp => sp.GetRequiredService<BrokerLifecycleRouter>());
        s.AddScoped<IBrokerOrderReader>(sp => sp.GetRequiredService<BrokerLifecycleRouter>());
        s.AddHttpClient<UpstoxMarketQuoteProvider>().AddStandardResilienceHandler();
        s.AddScoped<IBrokerAdapter>(sp => sp.GetRequiredService<UpstoxOrderGateway>());
        s.AddScoped<BrokerAdapterRegistry>();
        s.AddScoped<PaperTradingService>();
        s.AddScoped<ProductionTradingService>();
        s.AddScoped<TradingGateway>();
        s.AddScoped<IBrokerOrderGateway>(sp => sp.GetRequiredService<TradingGateway>());
        s.AddScoped<IOrderCancellationGateway>(sp => sp.GetRequiredService<TradingGateway>());
        s.AddScoped<IMarketQuoteProvider>(sp => sp.GetRequiredService<UpstoxMarketQuoteProvider>());
        return s;
    }
}
