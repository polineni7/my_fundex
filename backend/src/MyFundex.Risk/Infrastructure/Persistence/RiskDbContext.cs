using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MyFundex.BuildingBlocks.Abstractions;
using MyFundex.BuildingBlocks.Domain;
using MyFundex.BuildingBlocks.Persistence;
using MyFundex.Contracts;

namespace MyFundex.Risk;

public sealed class RiskDbContext(DbContextOptions<RiskDbContext> o, ICurrentActor a)
    : AuditableDbContext(o, a)
{
    public DbSet<PolicySet> PolicySets => Set<PolicySet>();
    public DbSet<PolicyVersion> PolicyVersions => Set<PolicyVersion>();
    public DbSet<PolicyRule> Rules => Set<PolicyRule>();
    public DbSet<PolicyAssignment> Assignments => Set<PolicyAssignment>();
    public DbSet<PolicyViolation> Violations => Set<PolicyViolation>();

    protected override void OnModelCreating(ModelBuilder m) => ConfigureModel(m);

    public static void ConfigureModel(ModelBuilder m)
    {
        m.HasDefaultSchema("fundex_risk");
        ConfigureEntity(m.Entity<PolicySet>());
        ConfigureEntity(m.Entity<PolicyVersion>());
        ConfigureEntity(m.Entity<PolicyRule>());
        ConfigureEntity(m.Entity<PolicyAssignment>());
        ConfigureEntity(m.Entity<PolicyViolation>());
        m.Entity<PolicyAssignment>()
            .HasIndex(x => new { x.FundedAccountInternalId, x.PolicyVersionInternalId })
            .IsUnique();
        m.Entity<PolicySet>().HasIndex(x => x.PolicyId).IsUnique();
        m.Entity<PolicySet>().HasIndex(x => x.Code).IsUnique().HasFilter("\"IsDeleted\" = false");
        m.Entity<PolicyRule>().Property(x => x.DecimalValue).HasPrecision(20, 6);
    }
}
