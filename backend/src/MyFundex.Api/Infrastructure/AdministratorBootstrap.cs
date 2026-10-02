using Microsoft.EntityFrameworkCore;
using MyFundex.Identity;

namespace MyFundex.Api.Infrastructure;

public static class AdministratorBootstrap
{
    public static async Task SeedAsync(
        IServiceProvider services,
        IConfiguration configuration,
        CancellationToken ct
    )
    {
        var email = configuration["Bootstrap:AdminEmail"]?.Trim().ToLowerInvariant();
        var password = configuration["Bootstrap:AdminPassword"];
        var errors = InputValidation.Registration(email, password, "System", "Administrator");
        if (errors.Count > 0)
            throw new InvalidOperationException(
                "Set Bootstrap:AdminEmail and a valid Bootstrap:AdminPassword before seeding an administrator."
            );
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<IdentityDbContext>();
        if (await db.Users.AnyAsync(x => x.Email == email, ct))
            throw new InvalidOperationException(
                "The requested bootstrap identity already exists; it will not be promoted automatically."
            );
        await using var transaction = await db.Database.BeginTransactionAsync(ct);
        var role = await db.Roles.SingleOrDefaultAsync(x => x.Code == "ADMIN", ct);
        if (role is null)
        {
            role = new Role
            {
                RoleId = Guid.NewGuid(),
                Code = "ADMIN",
                Name = "Administrator",
            };
            db.Add(role);
        }
        var user = new User
        {
            UserId = Guid.NewGuid(),
            Email = email!,
            Username = configuration["Bootstrap:AdminUsername"]?.Trim().ToLowerInvariant(),
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password!, 12),
            FirstName = "System",
            LastName = "Administrator",
        };
        db.Add(user);
        await db.SaveChangesAsync(ct);
        db.Add(new UserRole { UserInternalId = user.Id, RoleInternalId = role.Id });
        await db.SaveChangesAsync(ct);
        await transaction.CommitAsync(ct);
    }
}
