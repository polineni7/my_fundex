using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MyFundex.BuildingBlocks.Abstractions;
using MyFundex.BuildingBlocks.Domain;
using MyFundex.BuildingBlocks.Persistence;
using MyFundex.BuildingBlocks.Results;
using MyFundex.Contracts;

namespace MyFundex.Trading;

public sealed class TradingDbContext(DbContextOptions<TradingDbContext> o, ICurrentActor a)
    : AuditableDbContext(o, a)
{
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderEvent> OrderEvents => Set<OrderEvent>();
    public DbSet<Execution> Executions => Set<Execution>();

    protected override void OnModelCreating(ModelBuilder m) => ConfigureModel(m);

    public static void ConfigureModel(ModelBuilder m)
    {
        m.HasDefaultSchema("fundex_trading");
        ConfigureEntity(m.Entity<ProfitDistribution>());
        m.Entity<ProfitDistribution>().ToTable("ProfitDistributions", "myfund_prod_trading");
        m.Entity<ProfitDistribution>().HasIndex(x => x.DistributionId).IsUnique();
        m.Entity<ProfitDistribution>()
            .HasIndex(x => new { x.AccountId, x.SettlementReference })
            .IsUnique();
        ConfigureEntity(m.Entity<LiveBook>());
        ConfigureEntity(m.Entity<LivePosition>());
        m.Entity<LiveBook>().ToTable("Books", "myfund_prod_trading");
        m.Entity<LivePosition>().ToTable("Positions", "myfund_prod_trading");
        m.Entity<LiveBook>().HasIndex(x => x.AccountId).IsUnique();
        m.Entity<LivePosition>().HasIndex(x => new { x.AccountId, x.InstrumentToken }).IsUnique();
        m.Entity<LivePosition>().Property(x => x.AverageCost).HasPrecision(28, 10);
        ConfigureEntity(m.Entity<PaperBook>());
        ConfigureEntity(m.Entity<PaperPosition>());
        m.Entity<PaperBook>().ToTable("Books", "myfund_sandbox_trading");
        m.Entity<PaperPosition>().ToTable("Positions", "myfund_sandbox_trading");
        m.Entity<PaperBook>().HasIndex(x => x.AccountId).IsUnique();
        m.Entity<PaperPosition>().HasIndex(x => new { x.AccountId, x.InstrumentToken }).IsUnique();
        m.Entity<PaperPosition>().Property(x => x.AverageCost).HasPrecision(28, 10);
        ConfigureEntity(m.Entity<Order>());
        ConfigureEntity(m.Entity<OrderEvent>());
        ConfigureEntity(m.Entity<Execution>());
        m.Entity<Execution>()
            .HasIndex(x => new { x.OrderInternalId, x.BrokerExecutionId })
            .IsUnique();
        m.Entity<Execution>()
            .HasOne(x => x.Order)
            .WithMany()
            .HasForeignKey(x => x.OrderInternalId)
            .OnDelete(DeleteBehavior.Restrict);
        m.Entity<Order>().HasIndex(x => x.OrderId).IsUnique();
        m.Entity<Order>().HasIndex(x => x.IdempotencyKey).IsUnique();
        m.Entity<Order>().HasIndex(x => new { x.FundedAccountInternalId, x.CreatedAt });
        m.Entity<Order>().Property(x => x.Quantity).HasPrecision(20, 6);
        m.Entity<Order>().Property(x => x.EstimatedPrice).HasPrecision(20, 6);
    }
}
