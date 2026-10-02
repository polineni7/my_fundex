using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using MyFundex.BuildingBlocks.Abstractions;
using MyFundex.Identity;

namespace MyFundex.Api.Infrastructure;

public static class IdentityEndpoints
{
    public static async Task ValidateTokenAsync(TokenValidatedContext context)
    {
        var principal = context.Principal;
        if (
            !long.TryParse(principal?.FindFirstValue("internal_user_id"), out var id)
            || !long.TryParse(principal?.FindFirstValue("security_version"), out var version)
        )
        {
            context.Fail("Invalid session.");
            return;
        }
        var db = context.HttpContext.RequestServices.GetRequiredService<IdentityDbContext>();
        var valid = await db
            .Users.AsNoTracking()
            .AnyAsync(
                x => x.Id == id && x.Status == "Active" && x.SecurityVersion == version,
                context.HttpContext.RequestAborted
            );
        if (!valid)
            context.Fail("Session revoked.");
    }

    public static void MapIdentityManagement(this WebApplication app)
    {
        var me = app.MapGroup("/api/v1/me").RequireAuthorization();
        me.MapGet(
            "",
            async (IdentityDbContext db, ICurrentActor actor, ILoggerFactory logs, CancellationToken ct) =>
            {
                var user = await IdentityProfileReader.Credentials(db).SingleAsync(x => x.Id == actor.ActorId, ct);
                var profileAvailable = await IdentityProfileReader.ReadNamesAsync(db, user, logs.CreateLogger("IdentityProfile"), ct);
                var roles = await (
                    from link in db.UserRoles.AsNoTracking()
                    join role in db.Roles.AsNoTracking() on link.RoleInternalId equals role.Id
                    where link.UserInternalId == user.Id
                    select role.Code
                ).ToListAsync(ct);
                return Results.Ok(
                    new
                    {
                        user.UserId,
                        user.Email,
                        user.FirstName,
                        user.LastName,
                        user.Status,
                        user.Version,
                        roles,
                        hasGoogleLogin = user.GoogleSubject != null,
                        profileRecoveryRequired = !profileAvailable,
                    }
                );
            }
        );
        me.MapPut(
            "",
            async (
                ProfileInput input,
                IdentityDbContext db,
                ICurrentActor actor,
                CancellationToken ct
            ) =>
            {
                if (
                    string.IsNullOrWhiteSpace(input.FirstName)
                    || input.FirstName.Length > 100
                    || input.LastName == null
                    || input.LastName.Length > 100
                )
                    return Results.BadRequest(
                        new { message = "Names must be at most 100 characters." }
                    );
                var user = await IdentityProfileReader.Credentials(db).SingleAsync(x => x.Id == actor.ActorId, ct);
                db.Attach(user);
                if (user.Version != input.Version)
                    return Results.Conflict();
                user.FirstName = input.FirstName.Trim();
                user.LastName = input.LastName.Trim();
                db.Entry(user).Property(x => x.FirstName).IsModified = true;
                db.Entry(user).Property(x => x.LastName).IsModified = true;
                db.Events.Add(
                    new IdentityEvent
                    {
                        UserInternalId = user.Id,
                        EventType = "ProfileUpdated",
                        Method = "SelfService",
                        Succeeded = true,
                    }
                );
                await db.SaveChangesAsync(ct);
                return Results.Ok(new { user.Version });
            }
        );
        me.MapPost("/password", async (ChangePasswordInput input, IdentityDbContext db, ICurrentActor actor, CancellationToken ct) =>
        {
            if (string.IsNullOrEmpty(input.CurrentPassword) || string.IsNullOrEmpty(input.NewPassword) ||
                input.NewPassword.Length < 12 || System.Text.Encoding.UTF8.GetByteCount(input.NewPassword) > 72)
                return Results.BadRequest(new { message = "Use a new password of at least 12 characters and at most 72 UTF-8 bytes." });
            var user = await IdentityProfileReader.Credentials(db).SingleAsync(x => x.Id == actor.ActorId, ct);
                db.Attach(user);
            if (string.IsNullOrEmpty(user.PasswordHash) || !BCrypt.Net.BCrypt.Verify(input.CurrentPassword, user.PasswordHash))
                return Results.BadRequest(new { message = "The current password is incorrect." });
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(input.NewPassword, 12);
            user.SecurityVersion++;
            db.Events.Add(new IdentityEvent {UserInternalId=user.Id,EventType="PasswordChanged",Method="SelfService",Succeeded=true});
            await db.SaveChangesAsync(ct);
            return Results.NoContent();
        }).RequireRateLimiting("authentication");
        me.MapPost(
            "/logout-all",
            async (IdentityDbContext db, ICurrentActor actor, CancellationToken ct) =>
            {
                var user = await db.Users.SingleAsync(x => x.Id == actor.ActorId, ct);
                user.SecurityVersion++;
                db.Events.Add(
                    new IdentityEvent
                    {
                        UserInternalId = user.Id,
                        EventType = "SessionsRevoked",
                        Method = "SelfService",
                        Succeeded = true,
                    }
                );
                await db.SaveChangesAsync(ct);
                return Results.NoContent();
            }
        );
        var admin = app.MapGroup("/api/v1/admin/identity")
            .RequireAuthorization(p => p.RequireRole("ADMIN"));
        admin.MapGet(
            "/users",
            async (IdentityDbContext db, long? after, CancellationToken ct) =>
            {
                var users = await db
                    .Users.AsNoTracking()
                    .Where(x => x.Id > (after ?? 0))
                    .OrderBy(x => x.Id)
                    .Take(100)
                    .ToListAsync(ct);
                var ids = users.Select(x => x.Id).ToArray();
                var links = await (
                    from link in db.UserRoles.AsNoTracking()
                    join role in db.Roles.AsNoTracking() on link.RoleInternalId equals role.Id
                    where ids.Contains(link.UserInternalId)
                    select new { link.UserInternalId, role.Code }
                ).ToListAsync(ct);
                var roles = links.ToLookup(x => x.UserInternalId, x => x.Code);
                return Results.Ok(
                    new
                    {
                        items = users.Select(x => new
                        {
                            x.UserId,
                            x.Email,
                            x.FirstName,
                            x.LastName,
                            x.Status,
                            x.Version,
                            roles = roles[x.Id].ToArray(),
                            hasGoogleLogin = x.GoogleSubject != null,
                        }),
                        next = users.Count == 100 ? (long?)users[^1].Id : null,
                    }
                );
            }
        );
        admin.MapPost(
            "/users",
            async (CreateIdentityInput input, IdentityDbContext db, CancellationToken ct) =>
            {
                var errors = InputValidation.Registration(
                    input.Email,
                    input.Password,
                    input.FirstName,
                    input.LastName
                );
                if (
                    input.Roles == null
                    || input.Roles.Length == 0
                    || input.Roles.Any(x => x is not ("ADMIN" or "MANAGER" or "TRADER"))
                )
                    errors["roles"] = ["Select administrator, manager or trader roles."];
                if (errors.Count > 0)
                    return Results.ValidationProblem(errors);
                var email = input.Email.Trim().ToLowerInvariant();
                if (await db.Users.AnyAsync(x => x.Email == email, ct))
                    return Results.Conflict(
                        new { message = "An account already uses this email." }
                    );
                var requested = input.Roles!.Distinct().ToList();
                var roles = await db.Roles.Where(x => requested.Contains(x.Code)).ToListAsync(ct);
                if (roles.Count != requested.Count)
                    return Results.Problem("Role catalogue is incomplete.");
                var user = new User
                {
                    UserId = Guid.NewGuid(),
                    Email = email,
                    FirstName = input.FirstName.Trim(),
                    LastName = input.LastName.Trim(),
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(input.Password, 12),
                };
                await using var transaction = await db.Database.BeginTransactionAsync(ct);
                db.Add(user);
                foreach (var role in roles)
                    db.Add(new UserRole { User = user, RoleInternalId = role.Id });
                await db.SaveChangesAsync(ct);
                db.Events.Add(
                    new IdentityEvent
                    {
                        UserInternalId = user.Id,
                        EventType = "UserCreated",
                        Method = "Administration",
                        Succeeded = true,
                        Detail = string.Join(',', requested),
                    }
                );
                await db.SaveChangesAsync(ct);
                await transaction.CommitAsync(ct);
                return Results.Created(
                    $"/api/v1/admin/identity/users/{user.UserId}",
                    new
                    {
                        user.UserId,
                        user.Email,
                        roles = requested,
                    }
                );
            }
        );
        admin.MapGet(
            "/roles",
            async (IdentityDbContext db, CancellationToken ct) =>
                Results.Ok(
                    await db
                        .Roles.AsNoTracking()
                        .OrderBy(x => x.Code)
                        .Select(x => new
                        {
                            x.RoleId,
                            x.Code,
                            x.Name,
                        })
                        .ToListAsync(ct)
                )
        );
        admin.MapGet(
            "/permissions",
            async (IdentityDbContext db, CancellationToken ct) =>
                Results.Ok(
                    await (
                        from mapping in db.RolePermissions.AsNoTracking()
                        join role in db.Roles.AsNoTracking()
                            on mapping.RoleInternalId equals role.Id
                        join permission in db.Permissions.AsNoTracking()
                            on mapping.PermissionInternalId equals permission.Id
                        select new { role = role.Code, permission = permission.Code }
                    ).ToListAsync(ct)
                )
        );
        admin.MapPut(
            "/users/{userId:guid}/access",
            async (
                Guid userId,
                AccessInput input,
                IdentityAdministration service,
                CancellationToken ct
            ) =>
            {
                await service.UpdateAccessAsync(
                    userId,
                    input.Version,
                    input.Status,
                    input.Roles,
                    input.Reason,
                    ct
                );
                return Results.NoContent();
            }
        );
        admin.MapGet(
            "/users/{userId:guid}/history",
            async (Guid userId, IdentityDbContext db, CancellationToken ct) => Results.Ok(await (
                        from entry in db.Events.AsNoTracking()
                        join user in db.Users.AsNoTracking() on entry.UserInternalId equals user.Id
                        where user.UserId == userId
                        orderby entry.Id descending
                        select new
                        {
                            entry.EventId,
                            entry.EventType,
                            entry.Method,
                            entry.Succeeded,
                            entry.Detail,
                            entry.CreatedAt,
                            entry.CreatedBy,
                        }
                    ).Take(100).ToListAsync(ct))
        );
    }
}

public sealed record ProfileInput(string FirstName, string LastName, long Version);

public sealed record AccessInput(long Version, string Status, string[] Roles, string Reason);

public sealed record CreateIdentityInput(
    string Email,
    string Password,
    string FirstName,
    string LastName,
    string[] Roles
);

public sealed record ChangePasswordInput(string CurrentPassword, string NewPassword);
