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
        ConfigureEntity(m.Entity<Order>());
        ConfigureEntity(m.Entity<OrderEvent>());
        ConfigureEntity(m.Entity<Execution>());
        m.Entity<Order>().HasIndex(x => x.OrderId).IsUnique();
        m.Entity<Order>().HasIndex(x => x.IdempotencyKey).IsUnique();
        m.Entity<Order>().HasIndex(x => new { x.FundedAccountInternalId, x.CreatedAt });
        m.Entity<Order>().Property(x => x.Quantity).HasPrecision(20, 6);
        m.Entity<Order>().Property(x => x.EstimatedPrice).HasPrecision(20, 6);
    }
}
