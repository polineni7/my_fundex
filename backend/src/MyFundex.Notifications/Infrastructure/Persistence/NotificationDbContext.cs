using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MyFundex.BuildingBlocks.Abstractions;
using MyFundex.BuildingBlocks.Domain;
using MyFundex.BuildingBlocks.Persistence;
using MyFundex.Contracts;

namespace MyFundex.Notifications;

public sealed class NotificationDbContext(
    DbContextOptions<NotificationDbContext> o,
    ICurrentActor a
) : AuditableDbContext(o, a)
{
    public DbSet<Notification> Notifications => Set<Notification>();

    protected override void OnModelCreating(ModelBuilder m) => ConfigureModel(m);

    public static void ConfigureModel(ModelBuilder m)
    {
        m.HasDefaultSchema("fundex_notification");
        ConfigureEntity(m.Entity<Notification>());
        m.Entity<Notification>().HasIndex(x => x.NotificationId).IsUnique();
        m.Entity<Notification>().HasIndex(x => new { x.UserInternalId, x.IsRead });
    }
}
