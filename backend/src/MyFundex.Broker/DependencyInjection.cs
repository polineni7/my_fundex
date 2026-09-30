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
        s.AddDbContext<BrokerDbContext>(o => o.UseNpgsql(cs));
        s.AddHttpClient<UpstoxOrderGateway>();
        s.AddHttpClient<UpstoxMarketQuoteProvider>().AddStandardResilienceHandler();
        s.AddScoped<IBrokerOrderGateway>(sp => sp.GetRequiredService<UpstoxOrderGateway>());
        s.AddScoped<IMarketQuoteProvider>(sp => sp.GetRequiredService<UpstoxMarketQuoteProvider>());
        return s;
    }
}
