using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MyFundex.BuildingBlocks.Abstractions;
using MyFundex.BuildingBlocks.Domain;
using MyFundex.BuildingBlocks.Persistence;

namespace MyFundex.MarketData;

public sealed class MarketDataDbContext(DbContextOptions<MarketDataDbContext> o, ICurrentActor a)
    : AuditableDbContext(o, a)
{
    public DbSet<Instrument> Instruments => Set<Instrument>();
    public DbSet<TradingCalendarDay> TradingCalendar => Set<TradingCalendarDay>();

    protected override void OnModelCreating(ModelBuilder m) => ConfigureModel(m);

    public static void ConfigureModel(ModelBuilder m)
    {
        m.HasDefaultSchema("fundex_market");
        ConfigureEntity(m.Entity<Instrument>());
        ConfigureEntity(m.Entity<TradingCalendarDay>());
        m.Entity<Instrument>().HasIndex(x => x.InstrumentId).IsUnique();
        m.Entity<Instrument>()
            .HasIndex(x => x.InstrumentToken)
            .IsUnique()
            .HasFilter("\"IsDeleted\" = false");
        m.Entity<Instrument>().HasIndex(x => x.TradingSymbol);
        m.Entity<TradingCalendarDay>()
            .HasIndex(x => new { x.ExchangeCode, x.TradeDate })
            .IsUnique()
            .HasFilter("\"IsDeleted\" = false");
    }
}
