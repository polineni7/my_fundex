using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MyFundex.BuildingBlocks.Abstractions;
using MyFundex.BuildingBlocks.Domain;
using MyFundex.BuildingBlocks.Persistence;

namespace MyFundex.MarketData;

public static class MarketDataModule
{
    public static IServiceCollection AddMarketDataModule(this IServiceCollection s, string cs)
    {
        s.AddDbContext<MarketDataDbContext>(o => o.UseNpgsql(cs));
        return s;
    }
}
