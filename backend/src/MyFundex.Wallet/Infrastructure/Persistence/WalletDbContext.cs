using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MyFundex.BuildingBlocks.Abstractions;
using MyFundex.BuildingBlocks.Domain;
using MyFundex.BuildingBlocks.Persistence;
using MyFundex.Contracts;

namespace MyFundex.Wallet;

public sealed class WalletDbContext(DbContextOptions<WalletDbContext> o, ICurrentActor a)
    : AuditableDbContext(o, a)
{
    public DbSet<WalletAccount> Wallets => Set<WalletAccount>();
    public DbSet<LedgerTransaction> Transactions => Set<LedgerTransaction>();
    public DbSet<LedgerEntry> Entries => Set<LedgerEntry>();

    protected override void OnModelCreating(ModelBuilder m) => ConfigureModel(m);

    public static void ConfigureModel(ModelBuilder m)
    {
        m.HasDefaultSchema("fundex_wallet");
        ConfigureEntity(m.Entity<WalletAccount>());
        ConfigureEntity(m.Entity<LedgerTransaction>());
        ConfigureEntity(m.Entity<LedgerEntry>());
        m.Entity<LedgerTransaction>().HasIndex(x => x.PostingKey).IsUnique();
        m.Entity<WalletAccount>().HasIndex(x => x.WalletId).IsUnique();
        m.Entity<WalletAccount>()
            .HasIndex(x => x.FundedAccountInternalId)
            .IsUnique()
            .HasFilter("\"IsDeleted\" = false");
        m.Entity<LedgerEntry>().Property(x => x.Amount).HasPrecision(20, 4);
    }
}
