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
        s.AddScoped<LiveSettlementService>();
        s.AddScoped<LiveRiskMonitor>();
        s.AddScoped<SquareOffService>();
        s.AddScoped<ProfitDistributionService>();
        s.AddScoped<PaperTradingEngine>();
        s.AddScoped<IEvaluationProgressReader>(sp => sp.GetRequiredService<PaperTradingEngine>());
        s.AddScoped<CancelOrderService>();
        return s;
    }
}
