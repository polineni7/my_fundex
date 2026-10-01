using System.Text.Json;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using MyFundex.Broker;
using MyFundex.BuildingBlocks.Abstractions;
using MyFundex.Contracts;
using Xunit;

namespace MyFundex.Tests;
public sealed class BrokerConfigurationTests
{
    [Fact]
    public async Task CredentialsAreEncryptedEnvironmentSeparatedAndDisabledOrExpiredSessionsAreUnavailable()
    {
        await using var db = new BrokerDbContext(new DbContextOptionsBuilder<BrokerDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options, new Actor());
        var protection = new EphemeralDataProtectionProvider();
        var protector = protection.CreateProtector("MyFundex.Broker.Credentials.v1");
        var sandbox = new BrokerAccount { ProviderCode="Upstox", Environment="SANDBOX", AccountReference="sandbox",
            IsDefault=true, IsActive=true, ProtectedCredentials=protector.Protect(JsonSerializer.Serialize(new BrokerSecrets(null,null,"sandbox-test-token",null))) };
        var live = new BrokerAccount { ProviderCode="Upstox", Environment="PRODUCTION", AccountReference="live",
            IsActive=true, ProtectedCredentials=protector.Protect(JsonSerializer.Serialize(new BrokerSecrets(null,null,"live-test-token",null))) };
        db.AddRange(sandbox, live); await db.SaveChangesAsync();
        var service = new BrokerConfigurationService(db, protection, new Audit());
        Assert.DoesNotContain("sandbox-test-token", sandbox.ProtectedCredentials);
        Assert.Equal("sandbox-test-token", await service.GetAsync("Broker.Upstox.Sandbox.AccessToken",default));
        Assert.Equal("live-test-token", await service.GetAsync("Broker.Upstox.Accounts.live.AccessToken",default));
        Assert.Null(await service.GetAsync("Broker.Upstox.Accounts.sandbox.AccessToken",default));
        live.SessionExpiresAt=DateTimeOffset.UtcNow.AddMinutes(-1); await db.SaveChangesAsync();
        Assert.Null(await service.GetAsync("Broker.Upstox.Accounts.live.AccessToken",default));
        sandbox.IsActive=false; await db.SaveChangesAsync();
        Assert.Null(await service.GetAsync("Broker.Upstox.Sandbox.AccessToken",default));
    }
    private sealed class Actor : ICurrentActor {public long ActorId=>0;}
    private sealed class Audit : IAuditWriter {public Task WriteAsync(string module,string entityType,string entityId,string action,string? oldValue,string? newValue,CancellationToken ct)=>Task.CompletedTask;}
}
