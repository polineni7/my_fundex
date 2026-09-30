using Microsoft.EntityFrameworkCore;
using MyFundex.BuildingBlocks.Abstractions;
using MyFundex.BuildingBlocks.Domain;

namespace MyFundex.BuildingBlocks.Persistence;

public abstract class AuditableDbContext(DbContextOptions options, ICurrentActor currentActor)
    : DbContext(options)
{
    protected ICurrentActor CurrentActor { get; } = currentActor;

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder.Properties<decimal>().HavePrecision(20, 4);
    }

    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        ApplyAudit();
        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
        SaveChangesAsync(true, cancellationToken);

    public override Task<int> SaveChangesAsync(
        bool acceptAllChangesOnSuccess,
        CancellationToken cancellationToken = default
    )
    {
        ApplyAudit();
        return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }

    private void ApplyAudit()
    {
        var now = DateTimeOffset.UtcNow;
        var actor = CurrentActor.ActorId;
        foreach (var entry in ChangeTracker.Entries<EntityBase>())
        {
            if (
                entry.Entity is IImmutableRecord
                && entry.State is EntityState.Modified or EntityState.Deleted
            )
                throw new InvalidOperationException("Financial and audit records are append-only.");
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

    protected static void ConfigureEntity<TEntity>(
        Microsoft.EntityFrameworkCore.Metadata.Builders.EntityTypeBuilder<TEntity> b
    )
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
