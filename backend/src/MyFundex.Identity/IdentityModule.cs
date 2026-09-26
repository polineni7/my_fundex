using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MyFundex.BuildingBlocks.Abstractions;
using MyFundex.BuildingBlocks.Domain;
using MyFundex.BuildingBlocks.Persistence;

namespace MyFundex.Identity;

public sealed class User : EntityBase
{
    public Guid UserId { get; set; }
    public string Email { get; set; } = "";
    public string PasswordHash { get; set; } = "";
    public string FirstName { get; set; } = "";
    public string LastName { get; set; } = "";
    public string Status { get; set; } = "Active";
}
public sealed class Role : EntityBase { public Guid RoleId { get; set; } public string Code { get; set; } = ""; public string Name { get; set; } = ""; }
public sealed class Permission : EntityBase { public Guid PermissionId { get; set; } public string Code { get; set; } = ""; public string Name { get; set; } = ""; }
public sealed class UserRole : EntityBase { public long UserInternalId { get; set; } public long RoleInternalId { get; set; } }
public sealed class RolePermission : EntityBase { public long RoleInternalId { get; set; } public long PermissionInternalId { get; set; } }

public sealed class IdentityDbContext(DbContextOptions<IdentityDbContext> o, ICurrentActor a) : AuditableDbContext(o,a)
{
    public DbSet<User> Users => Set<User>(); public DbSet<Role> Roles => Set<Role>(); public DbSet<Permission> Permissions => Set<Permission>(); public DbSet<UserRole> UserRoles => Set<UserRole>(); public DbSet<RolePermission> RolePermissions => Set<RolePermission>();
    protected override void OnModelCreating(ModelBuilder m) => ConfigureModel(m); public static void ConfigureModel(ModelBuilder m) {
        m.HasDefaultSchema("identity");
        ConfigureEntity(m.Entity<User>()); ConfigureEntity(m.Entity<Role>()); ConfigureEntity(m.Entity<Permission>()); ConfigureEntity(m.Entity<UserRole>()); ConfigureEntity(m.Entity<RolePermission>());
        m.Entity<User>().HasIndex(x=>x.UserId).IsUnique(); m.Entity<User>().HasIndex(x=>x.Email).IsUnique().HasFilter("\"IsDeleted\" = false");
        m.Entity<Role>().HasIndex(x=>x.Code).IsUnique().HasFilter("\"IsDeleted\" = false");
        m.Entity<Permission>().HasIndex(x=>x.Code).IsUnique().HasFilter("\"IsDeleted\" = false");
    }
    static void ConfigureEntityDynamic(ModelBuilder m, Type t) => typeof(AuditableDbContext).GetMethod("ConfigureEntity", System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Static)!.MakeGenericMethod(t).Invoke(null,new object[]{m.Entity(t)});
}

public static class IdentityModule
{
    public static IServiceCollection AddIdentityModule(this IServiceCollection s, string cs)
    { s.AddDbContext<IdentityDbContext>(o=>o.UseNpgsql(cs)); return s; }
}
