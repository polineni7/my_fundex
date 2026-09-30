using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MyFundex.BuildingBlocks.Abstractions;
using MyFundex.BuildingBlocks.Domain;
using MyFundex.BuildingBlocks.Persistence;

namespace MyFundex.Intelligence;

public sealed class IntelligenceDbContext(
    DbContextOptions<IntelligenceDbContext> o,
    ICurrentActor a
) : AuditableDbContext(o, a)
{
    public DbSet<InstrumentSignal> Signals => Set<InstrumentSignal>();

    protected override void OnModelCreating(ModelBuilder m) => ConfigureModel(m);

    public static void ConfigureModel(ModelBuilder m)
    {
        m.HasDefaultSchema("fundex_intelligence");
        ConfigureEntity(m.Entity<InstrumentSignal>());
        m.Entity<InstrumentSignal>().HasIndex(x => x.SignalId).IsUnique();
        m.Entity<InstrumentSignal>().HasIndex(x => new { x.InstrumentToken, x.SignalTime });
    }
}
