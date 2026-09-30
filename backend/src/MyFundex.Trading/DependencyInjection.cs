using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MyFundex.BuildingBlocks.Abstractions;
using MyFundex.BuildingBlocks.Domain;
using MyFundex.BuildingBlocks.Persistence;
using MyFundex.BuildingBlocks.Results;
using MyFundex.Contracts;

namespace MyFundex.Trading;

public static class TradingModule
{
    public static IServiceCollection AddTradingModule(this IServiceCollection s, string cs)
    {
        s.AddDbContext<TradingDbContext>(o => o.UseNpgsql(cs));
        s.AddScoped<OrderService>();
        return s;
    }
}
