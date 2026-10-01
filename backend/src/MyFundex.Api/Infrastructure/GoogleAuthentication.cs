using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.EntityFrameworkCore;
using MyFundex.Identity;

namespace MyFundex.Api.Infrastructure;

public static class GoogleAuthentication
{
    private const string ExternalScheme = "GoogleExternal";

    public static IServiceCollection AddGoogleSignIn(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        var clientId = configuration["Authentication:Google:ClientId"];
        var clientSecret = configuration["Authentication:Google:ClientSecret"];
        if (string.IsNullOrWhiteSpace(clientId) || string.IsNullOrWhiteSpace(clientSecret))
            return services;

        services
            .AddAuthentication()
            .AddCookie(
                ExternalScheme,
                options =>
                {
                    options.Cookie.Name = "__Host-MyFundex.External";
                    options.Cookie.HttpOnly = true;
                    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
                    options.Cookie.SameSite = SameSiteMode.Lax;
                    options.ExpireTimeSpan = TimeSpan.FromMinutes(5);
                    options.SlidingExpiration = false;
                }
            )
            .AddGoogle(options =>
            {
                options.ClientId = clientId;
                options.ClientSecret = clientSecret;
                options.SignInScheme = ExternalScheme;
                options.UsePkce = true;
                options.SaveTokens = false;
                options.ClaimActions.MapJsonKey("email_verified", "verified_email");
                options.Events.OnRemoteFailure = context =>
                {
                    context.HandleResponse();
                    context.Response.Redirect("/api/v1/auth/google/failed");
                    return Task.CompletedTask;
                };
            });
        return services;
    }

    public static void MapGoogleSignIn(this WebApplication app)
    {
        app.MapGet(
            "/api/v1/auth/providers",
            (IConfiguration configuration) =>
                Results.Ok(
                    new
                    {
                        google = !string.IsNullOrWhiteSpace(
                            configuration["Authentication:Google:ClientId"]
                        )
                            && !string.IsNullOrWhiteSpace(
                                configuration["Authentication:Google:ClientSecret"]
                            ),
                    }
                )
        );
        app.MapGet(
            "/api/v1/auth/google",
            (IConfiguration configuration) =>
            {
                if (string.IsNullOrWhiteSpace(configuration["Authentication:Google:ClientId"]))
                    return Results.Problem("Google sign-in is not configured.", statusCode: 503);
                return Results.Challenge(
                    new AuthenticationProperties { RedirectUri = "/api/v1/auth/google/complete" },
                    [GoogleDefaults.AuthenticationScheme]
                );
            }
        );
        app.MapGet(
            "/api/v1/auth/google/failed",
            () => Results.Problem("Google sign-in could not be completed.", statusCode: 401)
        );
        app.MapGet(
            "/api/v1/auth/google/complete",
            async (HttpContext context, IConfiguration configuration) =>
            {
                var schemes =
                    context.RequestServices.GetRequiredService<IAuthenticationSchemeProvider>();
                if (await schemes.GetSchemeAsync(ExternalScheme) is null)
                    return Results.Problem("Google sign-in is not configured.", statusCode: 503);
                var result = await context.AuthenticateAsync(ExternalScheme);
                if (!result.Succeeded)
                    return Results.Unauthorized();
                var origin =
                    configuration["Authentication:TraderOrigin"] ?? "https://localhost:5173";
                if (!Uri.TryCreate(origin, UriKind.Absolute, out var uri) || uri.Scheme != "https")
                    return Results.Problem(
                        "A secure trader origin must be configured.",
                        statusCode: 503
                    );
                return Results.Redirect(origin.TrimEnd('/') + "/?google=complete");
            }
        );
        app.MapPost(
            "/api/v1/auth/google/exchange",
            async (
                HttpContext context,
                IdentityDbContext db,
                JwtTokenService tokens,
                CancellationToken ct
            ) =>
            {
                var schemes =
                    context.RequestServices.GetRequiredService<IAuthenticationSchemeProvider>();
                if (await schemes.GetSchemeAsync(ExternalScheme) is null)
                    return Results.Problem("Google sign-in is not configured.", statusCode: 503);
                var result = await context.AuthenticateAsync(ExternalScheme);
                if (!result.Succeeded || result.Principal is null)
                    return Results.Unauthorized();
                var subject = result.Principal.FindFirstValue(ClaimTypes.NameIdentifier);
                var email = result
                    .Principal.FindFirstValue(ClaimTypes.Email)
                    ?.Trim()
                    .ToLowerInvariant();
                var verified = result.Principal.FindFirstValue("email_verified");
                if (
                    string.IsNullOrWhiteSpace(subject)
                    || string.IsNullOrWhiteSpace(email)
                    || !string.Equals(verified, "true", StringComparison.OrdinalIgnoreCase)
                )
                    return Results.Unauthorized();
                var user = await db.Users.SingleOrDefaultAsync(x => x.GoogleSubject == subject, ct);
                if (user is null)
                {
                    // Never silently attach a Google identity to an existing password/admin account.
                    if (await db.Users.AnyAsync(x => x.Email == email, ct))
                        return Results.Conflict(
                            new
                            {
                                message = "This email already has an account. Use its existing sign-in method.",
                            }
                        );
                    user = new User
                    {
                        UserId = Guid.NewGuid(),
                        GoogleSubject = subject,
                        Email = email,
                        FirstName =
                            result.Principal.FindFirstValue(ClaimTypes.GivenName) ?? "Trader",
                        LastName = result.Principal.FindFirstValue(ClaimTypes.Surname) ?? "",
                        PasswordHash = "",
                    };
                    var traderRole = await db.Roles.SingleAsync(x => x.Code == "TRADER", ct);
                    db.Add(new UserRole { User = user, RoleInternalId = traderRole.Id });
                    db.Add(user);
                    await db.SaveChangesAsync(ct);
                }
                if (user.Status != "Active")
                    return Results.Unauthorized();
                db.Events.Add(
                    new IdentityEvent
                    {
                        UserInternalId = user.Id,
                        EventType = "Login",
                        Method = "Google",
                        Succeeded = true,
                    }
                );
                await db.SaveChangesAsync(ct);
                await context.SignOutAsync(ExternalScheme);
                // Google entry is trader-only; it cannot mint administrator or manager privileges.
                return Results.Ok(
                    new
                    {
                        accessToken = tokens.Create(user, ["TRADER"], []),
                        user = new
                        {
                            user.UserId,
                            user.Email,
                            user.FirstName,
                            user.LastName,
                            roles = new[] { "TRADER" },
                            permissions = Array.Empty<string>(),
                        },
                    }
                );
            }
        );
    }
}
