using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MyFundex.BuildingBlocks.Abstractions;
using MyFundex.BuildingBlocks.Domain;
using MyFundex.BuildingBlocks.Persistence;

namespace MyFundex.Portfolio;

public sealed class PortfolioDbContext(DbContextOptions<PortfolioDbContext> o, ICurrentActor a)
    : AuditableDbContext(o, a)
{
    public DbSet<Position> Positions => Set<Position>();
    public DbSet<PositionLot> Lots => Set<PositionLot>();

    protected override void OnModelCreating(ModelBuilder m) => ConfigureModel(m);

    public static void ConfigureModel(ModelBuilder m)
    {
        m.HasDefaultSchema("fundex_portfolio");
        ConfigureEntity(m.Entity<Position>());
        ConfigureEntity(m.Entity<PositionLot>());
        m.Entity<Position>().HasIndex(x => x.PositionId).IsUnique();
        m.Entity<Position>()
            .HasIndex(x => new { x.FundedAccountInternalId, x.InstrumentToken })
            .HasFilter("\"IsDeleted\" = false AND \"Status\"='Open'");
    }
}
