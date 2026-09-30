using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MyFundex.BuildingBlocks.Abstractions;
using MyFundex.BuildingBlocks.Domain;
using MyFundex.BuildingBlocks.Persistence;
using MyFundex.Contracts;

namespace MyFundex.FundedAccounts;

public sealed class AccountsDbContext(DbContextOptions<AccountsDbContext> o, ICurrentActor a)
    : AuditableDbContext(o, a)
{
    public DbSet<FundedAccount> Accounts => Set<FundedAccount>();
    public DbSet<CapitalAllocation> CapitalAllocations => Set<CapitalAllocation>();
    public DbSet<AccountStatusHistory> StatusHistory => Set<AccountStatusHistory>();

    protected override void OnModelCreating(ModelBuilder m) => ConfigureModel(m);

    public static void ConfigureModel(ModelBuilder m)
    {
        m.HasDefaultSchema("fundex_accounts");
        ConfigureEntity(m.Entity<FundedAccount>());
        ConfigureEntity(m.Entity<CapitalAllocation>());
        ConfigureEntity(m.Entity<AccountStatusHistory>());
        m.Entity<FundedAccount>().HasIndex(x => x.ProvisioningId).IsUnique();
        m.Entity<FundedAccount>().HasIndex(x => x.AccountId).IsUnique();
        m.Entity<FundedAccount>()
            .HasIndex(x => x.AccountNumber)
            .IsUnique()
            .HasFilter("\"IsDeleted\" = false");
        m.Entity<FundedAccount>().Property(x => x.FundedCapital).HasPrecision(20, 4);
        m.Entity<FundedAccount>().Property(x => x.CurrentBuyingPower).HasPrecision(20, 4);
    }
}
