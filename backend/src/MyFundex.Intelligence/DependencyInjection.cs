using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MyFundex.BuildingBlocks.Abstractions;
using MyFundex.BuildingBlocks.Domain;
using MyFundex.BuildingBlocks.Persistence;

namespace MyFundex.Intelligence;

public static class IntelligenceModule
{
    public static IServiceCollection AddIntelligenceModule(this IServiceCollection s, string cs)
    {
        s.AddDbContext<IntelligenceDbContext>(o => o.UseNpgsql(cs));
        return s;
    }
}
