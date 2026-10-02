using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MyFundex.BuildingBlocks.Abstractions;
using MyFundex.BuildingBlocks.Domain;
using MyFundex.BuildingBlocks.Persistence;

namespace MyFundex.Payments;

public sealed class PaymentsDbContext(DbContextOptions<PaymentsDbContext> o, ICurrentActor a)
    : AuditableDbContext(o, a)
{
    public DbSet<PaymentTransaction> Payments => Set<PaymentTransaction>();
    public DbSet<Coupon> Coupons => Set<Coupon>();
    public DbSet<Invoice> Invoices => Set<Invoice>();
    public DbSet<Receipt> Receipts => Set<Receipt>();

    protected override void OnModelCreating(ModelBuilder m) => ConfigureModel(m);

    public static void ConfigureModel(ModelBuilder m)
    {
        m.HasDefaultSchema("fundex_payments");
        ConfigureEntity(m.Entity<Coupon>());
        m.Entity<Coupon>().ToTable("Coupons", "fundex_payments");
        m.Entity<Coupon>().HasIndex(x => x.CouponId).IsUnique();
        m.Entity<Coupon>().HasIndex(x => x.Code).IsUnique();
        m.Entity<Coupon>().Property(x => x.Code).HasMaxLength(40);
        m.Entity<Coupon>().Property(x => x.Name).HasMaxLength(150);
        m.Entity<PaymentTransaction>().HasIndex(x => new { x.CouponId, x.UserInternalId });
        ConfigureEntity(m.Entity<PaymentTransaction>());
        ConfigureEntity(m.Entity<Invoice>());
        ConfigureEntity(m.Entity<Receipt>());
        m.Entity<PaymentTransaction>().HasIndex(x => x.PaymentId).IsUnique();
        m.Entity<PaymentTransaction>().HasIndex(x => x.IdempotencyKey).IsUnique();
        m.Entity<PaymentTransaction>().HasIndex(x => x.ProviderOrderId).IsUnique();
        m.Entity<PaymentTransaction>().HasIndex(x => x.ProviderPaymentId).IsUnique();
        m.Entity<PaymentTransaction>()
            .HasIndex(x => x.SubscriptionId)
            .IsUnique()
            .HasFilter("\"Status\" <> 'Failed' AND \"IsDeleted\" = false");
        m.Entity<Receipt>().HasIndex(x => x.PaymentInternalId).IsUnique();
        m.Entity<PaymentTransaction>().Property(x => x.Amount).HasPrecision(20, 4);
    }
}
