using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MyFundex.BuildingBlocks.Abstractions;
using MyFundex.BuildingBlocks.Domain;
using MyFundex.BuildingBlocks.Persistence;
using MyFundex.Contracts;

namespace MyFundex.Audit;

public static class AuditModule
{
    public static IServiceCollection AddAuditModule(this IServiceCollection s, string cs)
    {
        s.AddDbContext<AuditDbContext>(o => o.UseNpgsql(cs));
        s.AddScoped<IAuditWriter, AuditWriter>();
        return s;
    }
}
