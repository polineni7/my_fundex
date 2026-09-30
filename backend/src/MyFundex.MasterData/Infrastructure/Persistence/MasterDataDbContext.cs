using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MyFundex.BuildingBlocks.Abstractions;
using MyFundex.BuildingBlocks.Domain;
using MyFundex.BuildingBlocks.Persistence;

namespace MyFundex.MasterData;

public sealed class MasterDataDbContext(DbContextOptions<MasterDataDbContext> o, ICurrentActor a)
    : AuditableDbContext(o, a)
{
    public DbSet<Country> Countries => Set<Country>();
    public DbSet<Currency> Currencies => Set<Currency>();
    public DbSet<Exchange> Exchanges => Set<Exchange>();
    public DbSet<SecurityType> SecurityTypes => Set<SecurityType>();

    protected override void OnModelCreating(ModelBuilder m) => ConfigureModel(m);

    public static void ConfigureModel(ModelBuilder m)
    {
        m.HasDefaultSchema("fundex_master");
        ConfigureEntity(m.Entity<Country>());
        ConfigureEntity(m.Entity<Currency>());
        ConfigureEntity(m.Entity<Exchange>());
        ConfigureEntity(m.Entity<SecurityType>());
        m.Entity<Country>().HasIndex(x => x.Code).IsUnique().HasFilter("\"IsDeleted\" = false");
        m.Entity<Currency>().HasIndex(x => x.Code).IsUnique().HasFilter("\"IsDeleted\" = false");
        m.Entity<Exchange>().HasIndex(x => x.Code).IsUnique().HasFilter("\"IsDeleted\" = false");
        m.Entity<SecurityType>()
            .HasIndex(x => x.Code)
            .IsUnique()
            .HasFilter("\"IsDeleted\" = false");
    }
}
