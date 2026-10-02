using System.Security.Cryptography.X509Certificates;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.DataProtection.KeyManagement;

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
        if (string.Equals(builder.Configuration["DataProtection:KeyStore"], "Database", StringComparison.OrdinalIgnoreCase))
        {
            var certificate = LoadKeyCertificate(builder.Configuration);
            if (!certificate.HasPrivateKey)
                throw new InvalidOperationException("The Data Protection certificate must include its private key.");
            protection.ProtectKeysWithCertificate(certificate);
            var connection = builder.Configuration.GetConnectionString("Postgres")
                ?? throw new InvalidOperationException("Configure the PostgreSQL connection for shared key storage.");
            builder.Services.Configure<KeyManagementOptions>(options => options.XmlRepository = new PostgresKeyRepository(connection));
        }
        else if (builder.Environment.IsDevelopment())
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
    private static X509Certificate2 LoadKeyCertificate(IConfiguration configuration)
    {
        var thumbprint = configuration["DataProtection:CertificateThumbprint"];
        if (!string.IsNullOrWhiteSpace(thumbprint))
        {
            using var store = new X509Store(StoreName.My, StoreLocation.CurrentUser);
            store.Open(OpenFlags.ReadOnly);
            var certificates = store.Certificates.Find(X509FindType.FindByThumbprint, thumbprint, validOnly: false);
            if (certificates.Count != 1)
                throw new InvalidOperationException("The configured key-ring certificate is missing from the current user's certificate store.");
            return certificates[0];
        }
        var path = configuration["DataProtection:CertificatePath"];
        if (string.IsNullOrWhiteSpace(path))
            throw new InvalidOperationException("Database key storage requires an encryption certificate outside the database.");
        return new X509Certificate2(path, configuration["DataProtection:CertificatePassword"]);
    }

}
