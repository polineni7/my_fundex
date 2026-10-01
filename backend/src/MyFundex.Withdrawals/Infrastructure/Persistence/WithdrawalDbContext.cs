using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MyFundex.BuildingBlocks.Abstractions;
using MyFundex.BuildingBlocks.Domain;
using MyFundex.BuildingBlocks.Persistence;

namespace MyFundex.Withdrawals;

public sealed class WithdrawalDbContext(DbContextOptions<WithdrawalDbContext> o, ICurrentActor a)
    : AuditableDbContext(o, a)
{
    public DbSet<WithdrawalRequest> Requests => Set<WithdrawalRequest>();
    public DbSet<WithdrawalCalculation> Calculations => Set<WithdrawalCalculation>();

    protected override void OnModelCreating(ModelBuilder m) => ConfigureModel(m);

    public static void ConfigureModel(ModelBuilder m)
    {
        m.HasDefaultSchema("fundex_withdrawal");
        m.Entity<WithdrawalRequest>().HasIndex(x => new { x.Status, x.NextCheckAt });
        ConfigureEntity(m.Entity<WithdrawalRequest>());
        ConfigureEntity(m.Entity<WithdrawalCalculation>());
        m.Entity<WithdrawalRequest>().HasIndex(x => x.ProviderPayoutId).IsUnique();
        m.Entity<WithdrawalRequest>().HasIndex(x => new { x.Status, x.Id });
        m.Entity<WithdrawalRequest>().HasIndex(x => x.WithdrawalId).IsUnique();
    }
}
