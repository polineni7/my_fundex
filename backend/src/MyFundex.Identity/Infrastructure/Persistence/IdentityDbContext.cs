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
        ConfigureEntity(m.Entity<User>());
        ConfigureEntity(m.Entity<Role>());
        ConfigureEntity(m.Entity<Permission>());
        ConfigureEntity(m.Entity<UserRole>());
        ConfigureEntity(m.Entity<RolePermission>());
        m.Entity<User>().HasIndex(x => x.GoogleSubject).IsUnique();
        m.Entity<User>().HasIndex(x => x.UserId).IsUnique();
        m.Entity<User>().HasIndex(x => x.Email).IsUnique().HasFilter("\"IsDeleted\" = false");
        m.Entity<Role>().HasIndex(x => x.Code).IsUnique().HasFilter("\"IsDeleted\" = false");
        m.Entity<Permission>().HasIndex(x => x.Code).IsUnique().HasFilter("\"IsDeleted\" = false");
    }
}
