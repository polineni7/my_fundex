using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MyFundex.BuildingBlocks.Abstractions;
using MyFundex.BuildingBlocks.Domain;
using MyFundex.BuildingBlocks.Persistence;

namespace MyFundex.Subscription;

public sealed class SubscriptionDbContext(
    DbContextOptions<SubscriptionDbContext> o,
    ICurrentActor a
) : AuditableDbContext(o, a)
{
    public DbSet<Plan> Plans => Set<Plan>();
    public DbSet<PlanVersion> PlanVersions => Set<PlanVersion>();
    public DbSet<PlanStageDefinition> Stages => Set<PlanStageDefinition>();
    public DbSet<UserSubscription> Subscriptions => Set<UserSubscription>();

    protected override void OnModelCreating(ModelBuilder m) => ConfigureModel(m);

    public static void ConfigureModel(ModelBuilder m)
    {
        m.HasDefaultSchema("fundex_subscription");
        ConfigureEntity(m.Entity<ChallengeAttempt>());
        m.Entity<ChallengeAttempt>().ToTable("ChallengeAttempts", "fundex_subscription");
        m.Entity<ChallengeAttempt>().HasIndex(x => x.AttemptId).IsUnique();
        m.Entity<ChallengeAttempt>()
            .HasIndex(x => new { x.SubscriptionInternalId, x.StageInternalId })
            .IsUnique();
        ConfigureEntity(m.Entity<Plan>());
        ConfigureEntity(m.Entity<PlanVersion>());
        ConfigureEntity(m.Entity<PlanStageDefinition>());
        ConfigureEntity(m.Entity<UserSubscription>());
        m.Entity<Plan>().HasIndex(x => x.PlanId).IsUnique();
        m.Entity<Plan>().HasIndex(x => x.Code).IsUnique().HasFilter("\"IsDeleted\" = false");
        m.Entity<PlanVersion>()
            .HasIndex(x => new { x.PlanInternalId, x.VersionNumber })
            .IsUnique()
            .HasFilter("\"IsDeleted\" = false");
        m.Entity<UserSubscription>().HasIndex(x => x.SubscriptionId).IsUnique();
    }
}
