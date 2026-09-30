using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MyFundex.BuildingBlocks.Abstractions;
using MyFundex.BuildingBlocks.Domain;
using MyFundex.BuildingBlocks.Persistence;
using MyFundex.Contracts;

namespace MyFundex.Audit;

public sealed class AuditDbContext(DbContextOptions<AuditDbContext> o, ICurrentActor a)
    : AuditableDbContext(o, a)
{
    public DbSet<AuditEvent> Events => Set<AuditEvent>();

    protected override void OnModelCreating(ModelBuilder m) => ConfigureModel(m);

    public static void ConfigureModel(ModelBuilder m)
    {
        m.HasDefaultSchema("fundex_audit");
        ConfigureEntity(m.Entity<AuditEvent>());
        m.Entity<AuditEvent>().HasIndex(x => x.AuditId).IsUnique();
        m.Entity<AuditEvent>().HasIndex(x => new { x.Module, x.OccurredAt });
    }
}
