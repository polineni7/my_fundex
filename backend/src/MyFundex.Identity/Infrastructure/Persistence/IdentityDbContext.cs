using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MyFundex.BuildingBlocks.Abstractions;
using MyFundex.BuildingBlocks.Domain;
using MyFundex.BuildingBlocks.Persistence;

namespace MyFundex.Identity;

public sealed class IdentityDbContext(
    DbContextOptions<IdentityDbContext> o,
    ICurrentActor a,
    IDataProtectionProvider protection
) : AuditableDbContext(o, a)
{
    public DbSet<IdentityEvent> Events => Set<IdentityEvent>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<Permission> Permissions => Set<Permission>();
    public DbSet<UserRole> UserRoles => Set<UserRole>();
    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();

    protected override void OnModelCreating(ModelBuilder m)
    {
        ConfigureModel(m);
        var protector = protection.CreateProtector("MyFundex.Identity.Profile.v1");
        m.Entity<User>()
            .Property(x => x.FirstName)
            .HasConversion(value => protector.Protect(value), value => protector.Unprotect(value));
        m.Entity<User>()
            .Property(x => x.LastName)
            .HasConversion(value => protector.Protect(value), value => protector.Unprotect(value));
    }

    public static void ConfigureModel(ModelBuilder m)
    {
        m.HasDefaultSchema("fundex_identity");
        ConfigureEntity(m.Entity<IdentityEvent>());
        m.Entity<IdentityEvent>().ToTable("IdentityEvents", "fundex_identity");
        m.Entity<IdentityEvent>().HasIndex(x => x.EventId).IsUnique();
        m.Entity<IdentityEvent>().HasIndex(x => new { x.UserInternalId, x.CreatedAt });
        m.Entity<IdentityEvent>().Property(x => x.EventType).HasMaxLength(60);
        m.Entity<IdentityEvent>().Property(x => x.Method).HasMaxLength(30);
        m.Entity<IdentityEvent>().Property(x => x.Detail).HasMaxLength(500);
        m.Entity<IdentityEvent>()
            .HasOne<User>()
            .WithMany()
            .HasForeignKey(x => x.UserInternalId)
            .OnDelete(DeleteBehavior.Restrict);
        m.Entity<UserRole>()
            .HasOne(x => x.User)
            .WithMany()
            .HasForeignKey(x => x.UserInternalId)
            .OnDelete(DeleteBehavior.Restrict);
        m.Entity<UserRole>()
            .HasOne<Role>()
            .WithMany()
            .HasForeignKey(x => x.RoleInternalId)
            .OnDelete(DeleteBehavior.Restrict);
        m.Entity<RolePermission>()
            .HasOne<Role>()
            .WithMany()
            .HasForeignKey(x => x.RoleInternalId)
            .OnDelete(DeleteBehavior.Restrict);
        m.Entity<RolePermission>()
            .HasOne<Permission>()
            .WithMany()
            .HasForeignKey(x => x.PermissionInternalId)
            .OnDelete(DeleteBehavior.Restrict);
        m.Entity<UserRole>()
            .HasIndex(x => new { x.UserInternalId, x.RoleInternalId })
            .IsUnique()
            .HasFilter("\"IsDeleted\" = false");
        m.Entity<RolePermission>()
            .HasIndex(x => new { x.RoleInternalId, x.PermissionInternalId })
            .IsUnique()
            .HasFilter("\"IsDeleted\" = false");
        m.Entity<Role>().HasIndex(x => x.RoleId).IsUnique();
        m.Entity<Permission>().HasIndex(x => x.PermissionId).IsUnique();
        ConfigureEntity(m.Entity<User>());
        ConfigureEntity(m.Entity<Role>());
        ConfigureEntity(m.Entity<Permission>());
        ConfigureEntity(m.Entity<UserRole>());
        ConfigureEntity(m.Entity<RolePermission>());
        m.Entity<User>().Property(x => x.Username).HasMaxLength(100);
        m.Entity<User>().HasIndex(x => x.Username).IsUnique().HasFilter("\"IsDeleted\" = false AND \"Username\" IS NOT NULL");
        m.Entity<User>().HasIndex(x => x.GoogleSubject).IsUnique();
        m.Entity<User>().HasIndex(x => x.UserId).IsUnique();
        m.Entity<User>().HasIndex(x => x.Email).IsUnique().HasFilter("\"IsDeleted\" = false");
        m.Entity<Role>().HasIndex(x => x.Code).IsUnique().HasFilter("\"IsDeleted\" = false");
        m.Entity<Permission>().HasIndex(x => x.Code).IsUnique().HasFilter("\"IsDeleted\" = false");
    }
}
