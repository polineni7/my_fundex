using Microsoft.EntityFrameworkCore;
using MyFundex.BuildingBlocks.Abstractions;
using MyFundex.BuildingBlocks.Domain;

namespace MyFundex.BuildingBlocks.Persistence;

public abstract class AuditableDbContext(DbContextOptions options, ICurrentActor currentActor) : DbContext(options)
{
    protected ICurrentActor CurrentActor { get; } = currentActor;

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        ApplyAudit();
        return base.SaveChangesAsync(cancellationToken);
    }

    private void ApplyAudit()
    {
        var now = DateTimeOffset.UtcNow;
        var actor = CurrentActor.ActorId;
        foreach (var entry in ChangeTracker.Entries<EntityBase>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedAt = now;
                    entry.Entity.CreatedBy = actor;
                    entry.Entity.IsDeleted = false;
                    entry.Entity.Version = Math.Max(1, entry.Entity.Version);
                    break;
                case EntityState.Modified:
                    entry.Entity.UpdatedAt = now;
                    entry.Entity.UpdatedBy = actor;
                    entry.Entity.Version++;
                    break;
                case EntityState.Deleted:
                    entry.State = EntityState.Modified;
                    entry.Entity.IsDeleted = true;
                    entry.Entity.DeletedAt = now;
                    entry.Entity.DeletedBy = actor;
                    entry.Entity.Version++;
                    break;
            }
        }
    }

    protected static void ConfigureEntity<TEntity>(Microsoft.EntityFrameworkCore.Metadata.Builders.EntityTypeBuilder<TEntity> b)
        where TEntity : EntityBase
    {
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).ValueGeneratedOnAdd();
        b.Property(x => x.CreatedAt).HasColumnType("timestamptz");
        b.Property(x => x.UpdatedAt).HasColumnType("timestamptz");
        b.Property(x => x.DeletedAt).HasColumnType("timestamptz");
        b.Property(x => x.Version).IsConcurrencyToken();
        b.HasQueryFilter(x => !x.IsDeleted);
    }
}
