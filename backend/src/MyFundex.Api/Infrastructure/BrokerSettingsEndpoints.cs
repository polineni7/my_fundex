using Microsoft.EntityFrameworkCore;
using MyFundex.Broker;
using MyFundex.Contracts;

namespace MyFundex.Api.Infrastructure;
public static class BrokerSettingsEndpoints
{
    public static void MapBrokerSettings(this WebApplication app)
    {
        var group = app.MapGroup("/api/v1/admin/broker-settings").RequireAuthorization(p => p.RequireRole("ADMIN"));
        group.MapGet("", async (BrokerDbContext db, CancellationToken ct) => Results.Ok(await db.Accounts.AsNoTracking()
            .OrderBy(x => x.ProviderCode).ThenBy(x => x.Environment).Select(x => new {
                x.BrokerAccountId, x.DisplayName, provider = x.ProviderCode, x.Environment, reference = x.AccountReference,
                x.BrokerUserId, x.IsActive, x.IsDefault, x.UseForMarketData, x.SessionExpiresAt, x.Version,
                hasCredentials = x.ProtectedCredentials != null, executionSupported = x.ProviderCode == "Upstox"
            }).ToListAsync(ct)));
        group.MapPost("", (BrokerSetupInput input, BrokerConfigurationService service, CancellationToken ct) =>
            SaveAsync(null, input, service, ct));
        group.MapPut("/{id:guid}", (Guid id, BrokerSetupInput input, BrokerConfigurationService service, CancellationToken ct) =>
            SaveAsync(id, input, service, ct));
        group.MapPost("/{id:guid}/verify", async (Guid id, BrokerDbContext db, IBrokerAccountVerifier verifier, CancellationToken ct) => {
            var account = await db.Accounts.AsNoTracking().SingleOrDefaultAsync(x => x.BrokerAccountId == id, ct);
            if (account == null) return Results.NotFound();
            if (!account.IsActive || account.Environment != "PRODUCTION" || account.ProviderCode != "Upstox")
                return Results.BadRequest(new { message = "Verification requires an enabled Upstox production connection." });
            var valid = await verifier.VerifyAsync(account.ProviderCode, account.AccountReference, account.BrokerUserId, ct);
            return valid ? Results.Ok(new { message = "Broker identity verified. No order was placed." })
                : Results.BadRequest(new { message = "Broker verification failed. Check the session token, expiry and broker user ID." });
        });
    }
    private static async Task<IResult> SaveAsync(Guid? id, BrokerSetupInput input,
        BrokerConfigurationService service, CancellationToken ct)
    {
        try
        {
            var account = await service.SaveAsync(id, input, ct);
            return Results.Ok(new { account.BrokerAccountId, account.Version });
        }
        catch (ArgumentException error)
        {
            return Results.BadRequest(new { message = error.Message });
        }
    }

}
