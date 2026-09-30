using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using MyFundex.BuildingBlocks.Abstractions;
using MyFundex.BuildingBlocks.Domain;
using MyFundex.BuildingBlocks.Persistence;
using MyFundex.Contracts;

namespace MyFundex.Configuration;

public static class ConfigurationModule
{
    public static IServiceCollection AddConfigurationModule(this IServiceCollection s, string cs)
    {
        s.AddMemoryCache();
        s.AddDbContext<ConfigurationDbContext>(o => o.UseNpgsql(cs));
        s.AddScoped<IRuntimeSettings, RuntimeSettings>();
        return s;
    }
}
