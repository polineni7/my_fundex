using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using MyFundex.Identity;

namespace MyFundex.Api.Infrastructure;

public static class IdentityProfileReader
{
    // Authentication must not require decrypting optional profile fields.
    public static IQueryable<User> Credentials(IdentityDbContext db) => db.Users.AsNoTracking().Select(x => new User
    {
        Id = x.Id, UserId = x.UserId, Email = x.Email, Username = x.Username, PasswordHash = x.PasswordHash,
        Status = x.Status, SecurityVersion = x.SecurityVersion, Version = x.Version,
        GoogleSubject = x.GoogleSubject
    });

    public static async Task<bool> ReadNamesAsync(IdentityDbContext db, User user, ILogger logger, CancellationToken ct)
    {
        try
        {
            var names = await db.Users.AsNoTracking().Where(x => x.Id == user.Id)
                .Select(x => new { x.FirstName, x.LastName }).SingleAsync(ct);
            user.FirstName = names.FirstName;
            user.LastName = names.LastName;
            return true;
        }
        catch (CryptographicException)
        {
            logger.LogWarning("Profile encryption key unavailable for user {UserId}. Original encrypted values are preserved.", user.UserId);
            user.FirstName = "";
            user.LastName = "";
            return false;
        }
    }
}
