using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MyFundex.BuildingBlocks.Abstractions;
using MyFundex.BuildingBlocks.Domain;
using MyFundex.BuildingBlocks.Persistence;

namespace MyFundex.Portfolio;

public static class PortfolioModule
{
    public static IServiceCollection AddPortfolioModule(this IServiceCollection s, string cs)
    {
        s.AddDbContext<PortfolioDbContext>(o => o.UseNpgsql(cs));
        return s;
    }
}
