using System.Security.Cryptography.X509Certificates;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.DataProtection;

namespace MyFundex.Api.Infrastructure;

public static class SecurityConfiguration
{
    public static void AddPlatformSecurity(this WebApplicationBuilder builder)
    {
        builder.Logging.ClearProviders();
        builder.Logging.AddJsonConsole();
        builder.WebHost.ConfigureKestrel(options => options.Limits.MaxRequestBodySize = 65_536);
        var protection = builder.Services.AddDataProtection().SetApplicationName("MyFundex");
        var directory = builder.Configuration["DataProtection:KeyDirectory"];
        if (builder.Environment.IsDevelopment())
        {
            directory ??= Path.Combine(builder.Environment.ContentRootPath, ".local", "keys");
            protection.PersistKeysToFileSystem(new DirectoryInfo(directory));
            if (OperatingSystem.IsWindows())
                protection.ProtectKeysWithDpapi();
        }
        else
        {
            var certificatePath = builder.Configuration["DataProtection:CertificatePath"];
            if (string.IsNullOrWhiteSpace(directory) || string.IsNullOrWhiteSpace(certificatePath))
                throw new InvalidOperationException(
                    "Configure persistent DataProtection keys and their encryption certificate for profile encryption and Google sign-in."
                );
            protection.PersistKeysToFileSystem(new DirectoryInfo(directory));
            protection.ProtectKeysWithCertificate(
                new X509Certificate2(
                    certificatePath,
                    builder.Configuration["DataProtection:CertificatePassword"]
                )
            );
        }
        builder.Services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            options.AddPolicy(
                "authentication",
                context =>
                    RateLimitPartition.GetFixedWindowLimiter(
                        context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                        _ => new FixedWindowRateLimiterOptions
                        {
                            PermitLimit = 15,
                            Window = TimeSpan.FromMinutes(1),
                            QueueLimit = 0,
                        }
                    )
            );
        });
    }
}
