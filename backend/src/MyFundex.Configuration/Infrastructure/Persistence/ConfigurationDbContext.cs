using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using MyFundex.BuildingBlocks.Abstractions;
using MyFundex.BuildingBlocks.Domain;
using MyFundex.BuildingBlocks.Persistence;
using MyFundex.Contracts;

namespace MyFundex.Configuration;

public sealed class ConfigurationDbContext(
    DbContextOptions<ConfigurationDbContext> o,
    ICurrentActor a
) : AuditableDbContext(o, a)
{
    public DbSet<SystemSetting> Settings => Set<SystemSetting>();

    protected override void OnModelCreating(ModelBuilder m) => ConfigureModel(m);

    public static void ConfigureModel(ModelBuilder m)
    {
        m.HasDefaultSchema("fundex_configuration");
        ConfigureEntity(m.Entity<SystemSetting>());
        m.Entity<SystemSetting>()
            .HasIndex(x => new { x.SettingKey, x.Environment })
            .IsUnique()
            .HasFilter("\"IsDeleted\" = false");
    }
}
