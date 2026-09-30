using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MyFundex.BuildingBlocks.Abstractions;
using MyFundex.BuildingBlocks.Domain;
using MyFundex.BuildingBlocks.Persistence;
using MyFundex.Contracts;

namespace MyFundex.Risk;

public static class RiskModule
{
    public static IServiceCollection AddRiskModule(this IServiceCollection s, string cs)
    {
        s.AddDbContext<RiskDbContext>(o => o.UseNpgsql(cs));
        s.AddScoped<IRiskGuard, RiskGuard>();
        s.AddScoped<IPolicyAssignmentService, PolicyAssignmentService>();
        s.AddScoped<IPolicyCatalogue, PolicyCatalogue>();
        return s;
    }
}
