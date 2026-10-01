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
        ConfigureEntity(m.Entity<ChallengeNotice>());
        m.Entity<ChallengeNotice>().ToTable("ChallengeNotices", "fundex_subscription");
        m.Entity<ChallengeNotice>()
            .HasIndex(x => new { x.SubscriptionInternalId, x.Outcome })
            .IsUnique();
        m.Entity<ChallengeNotice>().HasIndex(x => new { x.DeliveredAt, x.LeaseUntil });
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
        // Legacy CLR/API names are retained for existing clients; storage names reflect assessment enrolment.
        m.Entity<UserSubscription>().ToTable("AssessmentEnrollments", "fundex_subscription");
        m.Entity<UserSubscription>().Property(x => x.SubscriptionId).HasColumnName("EnrollmentId");
        m.Entity<UserSubscription>()
            .Property(x => x.PreviousSubscriptionId)
            .HasColumnName("PreviousEnrollmentId");
        m.Entity<UserSubscription>().Property(x => x.SubscribedAt).HasColumnName("EnrolledAt");
        m.Entity<PlanVersion>().Property(x => x.RegistrationFee).HasColumnName("AssessmentFee");
        m.Entity<Plan>().HasIndex(x => x.PlanId).IsUnique();
        m.Entity<Plan>().HasIndex(x => x.Code).IsUnique().HasFilter("\"IsDeleted\" = false");
        m.Entity<PlanVersion>()
            .HasIndex(x => new { x.PlanInternalId, x.VersionNumber })
            .IsUnique()
            .HasFilter("\"IsDeleted\" = false");
        m.Entity<UserSubscription>().HasIndex(x => x.PurchaseRequestId).IsUnique();
        m.Entity<UserSubscription>().HasIndex(x => x.SubscriptionId).IsUnique();
    }
}
