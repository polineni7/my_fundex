using System.Data;
using Microsoft.EntityFrameworkCore;
using MyFundex.BuildingBlocks.Abstractions;

namespace MyFundex.Identity;

public sealed class IdentityAdministration(IdentityDbContext db, ICurrentActor actor)
{
    public async Task UpdateAccessAsync(
        Guid userId,
        long version,
        string status,
        string[] roles,
        string reason,
        CancellationToken ct
    )
    {
        if (
            status is not ("Active" or "Suspended")
            || roles == null
            || roles.Length == 0
            || roles.Any(x => x is not ("TRADER" or "MANAGER" or "ADMIN"))
            || string.IsNullOrWhiteSpace(reason)
            || reason.Length > 300
        )
            throw new ArgumentException("Choose valid roles, status and a reason.");
        // Serializes the last-administrator invariant across concurrent administrators.
        await using var transaction = await db.Database.BeginTransactionAsync(
            IsolationLevel.Serializable,
            ct
        );
        var user =
            await db.Users.SingleOrDefaultAsync(x => x.UserId == userId, ct)
            ?? throw new ArgumentException("User not found.");
        if (user.Id == actor.ActorId)
            throw new ArgumentException("An administrator cannot change their own access here.");
        if (user.Version != version)
            throw new DbUpdateConcurrencyException();
        var desired = roles.Distinct(StringComparer.Ordinal).Order().ToArray();
        var selected = await db.Roles.Where(x => desired.Contains(x.Code)).ToListAsync(ct);
        if (selected.Count != desired.Length)
            throw new ArgumentException("Role catalogue is incomplete.");
        var assignments = await db
            .UserRoles.Where(x => x.UserInternalId == user.Id)
            .ToListAsync(ct);
        var adminId = await db
            .Roles.Where(x => x.Code == "ADMIN")
            .Select(x => x.Id)
            .SingleAsync(ct);
        if (
            assignments.Any(x => x.RoleInternalId == adminId)
            && (status != "Active" || !desired.Contains("ADMIN"))
        )
        {
            var otherAdmin = await (
                from link in db.UserRoles
                join member in db.Users on link.UserInternalId equals member.Id
                where
                    link.RoleInternalId == adminId
                    && member.Id != user.Id
                    && member.Status == "Active"
                select member.Id
            ).AnyAsync(ct);
            if (!otherAdmin)
                throw new ArgumentException("Keep at least one active administrator.");
        }
        var ids = selected.Select(x => x.Id).ToHashSet();
        foreach (var assignment in assignments.Where(x => !ids.Contains(x.RoleInternalId)))
            db.Remove(assignment);
        foreach (var role in selected.Where(x => assignments.All(a => a.RoleInternalId != x.Id)))
            db.Add(new UserRole { UserInternalId = user.Id, RoleInternalId = role.Id });
        user.Status = status;
        user.SecurityVersion++;
        db.Events.Add(
            new IdentityEvent
            {
                UserInternalId = user.Id,
                EventType = "AccessChanged",
                Method = "Administration",
                Succeeded = true,
                Detail = status + ":" + string.Join(',', desired) + ":" + reason.Trim(),
            }
        );
        await db.SaveChangesAsync(ct);
        await transaction.CommitAsync(ct);
    }
}
