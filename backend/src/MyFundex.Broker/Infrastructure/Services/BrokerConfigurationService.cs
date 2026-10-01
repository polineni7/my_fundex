using System.Text.Json;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using MyFundex.Contracts;

namespace MyFundex.Broker;

public sealed record BrokerSecrets(string? ApiKey, string? ApiSecret, string? AccessToken, string? RefreshToken);
public sealed record BrokerSetupInput(string DisplayName, string Provider, string Environment, string Reference,
    string BrokerUserId, bool IsActive, bool IsDefault, bool UseForMarketData, DateTimeOffset? SessionExpiresAt,
    string? ApiKey, string? ApiSecret, string? AccessToken, string? RefreshToken, long Version = 0);

public sealed class BrokerConfigurationService(BrokerDbContext db, IDataProtectionProvider protection, IAuditWriter audit)
    : IRuntimeSettingSource
{
    private readonly IDataProtector protector = protection.CreateProtector("MyFundex.Broker.Credentials.v1");
    public bool Handles(string key) => key.StartsWith("Broker.", StringComparison.Ordinal);
    public async Task<string?> GetAsync(string key, CancellationToken ct)
    {
        var parts = key.Split('.');
        if (parts.Length < 4) return null;
        var provider = parts[1];
        var query = db.Accounts.AsNoTracking().Where(x => x.ProviderCode == provider && x.IsActive);
        query = parts[2] switch
        {
            "Accounts" when parts.Length == 5 => query.Where(x => x.Environment == "PRODUCTION" && x.AccountReference == parts[3]),
            "Sandbox" => query.Where(x => x.Environment == "SANDBOX" && x.IsDefault),
            "MarketData" => query.Where(x => x.UseForMarketData),
            _ => query.Where(x => false)
        };
        var account = await query.SingleOrDefaultAsync(ct);
        if (account == null || account.SessionExpiresAt <= DateTimeOffset.UtcNow) return null;
        var secrets = ReadSecrets(account);
        return parts[^1] switch { "AccessToken" => secrets.AccessToken, "ApiKey" => secrets.ApiKey,
            "ApiSecret" => secrets.ApiSecret, "RefreshToken" => secrets.RefreshToken, _ => null };
    }
    public BrokerSecrets ReadSecrets(BrokerAccount account) => account.ProtectedCredentials == null
        ? new(null, null, null, null)
        : JsonSerializer.Deserialize<BrokerSecrets>(protector.Unprotect(account.ProtectedCredentials))!;

    public async Task<BrokerAccount> SaveAsync(Guid? id, BrokerSetupInput input, CancellationToken ct)
    {
        if (input.Provider is not ("Upstox" or "AngelOne" or "Groww") || input.Environment is not ("SANDBOX" or "PRODUCTION"))
            throw new ArgumentException("Select a supported broker and trading environment.");
        if (string.IsNullOrWhiteSpace(input.DisplayName) || input.DisplayName.Length > 100 ||
            string.IsNullOrWhiteSpace(input.Reference) || input.Reference.Length > 100 ||
            input.Reference.Any(c => !char.IsAsciiLetterOrDigit(c) && c != '-' && c != '_') || input.BrokerUserId == null || input.BrokerUserId.Length > 100)
            throw new ArgumentException("Provide a name and a credential reference containing only letters, numbers, hyphens or underscores.");
        if (new[] {input.ApiKey, input.ApiSecret, input.AccessToken, input.RefreshToken}.Any(x => x?.Length > 8000))
            throw new ArgumentException("Credential value is too long.");
        if (input.IsActive && input.SessionExpiresAt <= DateTimeOffset.UtcNow)
            throw new ArgumentException("An active connection cannot have an expired session.");
        if (input.UseForMarketData && input.Environment != "PRODUCTION")
            throw new ArgumentException("Live market data requires a production broker session. Assessment orders remain simulated.");
        await using var transaction = await db.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable, ct);
        var account = id.HasValue ? await db.Accounts.SingleOrDefaultAsync(x => x.BrokerAccountId == id, ct)
            ?? throw new ArgumentException("Broker configuration was not found.") : new BrokerAccount { BrokerAccountId = Guid.NewGuid() };
        if (id.HasValue && account.Version != input.Version) throw new DbUpdateConcurrencyException();
        if (id.HasValue && (account.ProviderCode != input.Provider || account.Environment != input.Environment || account.AccountReference != input.Reference))
            throw new ArgumentException("Provider, environment and reference cannot change. Create a separate connection instead.");
        if (await db.Accounts.AnyAsync(x => x.BrokerAccountId != account.BrokerAccountId && x.ProviderCode == input.Provider &&
            x.Environment == input.Environment && x.AccountReference == input.Reference, ct))
            throw new ArgumentException("This broker reference already exists in the selected environment.");
        if (input.IsActive && await db.Accounts.AnyAsync(x => x.BrokerAccountId != account.BrokerAccountId && x.IsActive &&
            ((input.IsDefault && x.IsDefault && x.Environment == input.Environment) || (input.UseForMarketData && x.UseForMarketData)), ct))
            throw new ArgumentException("Disable the existing default or market-data selection before selecting another connection.");
        var old = ReadSecrets(account);
        var secrets = new BrokerSecrets(Keep(input.ApiKey, old.ApiKey), Keep(input.ApiSecret, old.ApiSecret),
            Keep(input.AccessToken, old.AccessToken), Keep(input.RefreshToken, old.RefreshToken));
        if (input.IsActive && string.IsNullOrWhiteSpace(secrets.AccessToken))
            throw new ArgumentException("An access/session token is required to enable this configuration.");
        // Non-Upstox execution must remain disabled until complete reconciliation and instrument mapping adapters exist.
        if (input.IsActive && input.Provider != "Upstox")
            throw new ArgumentException("Credentials can be saved, but this provider's trading adapter is not ready. Keep the connection disabled.");
        account.DisplayName = input.DisplayName.Trim(); account.ProviderCode = input.Provider;
        account.Environment = input.Environment; account.AccountReference = input.Reference;
        account.BrokerUserId = input.BrokerUserId.Trim(); account.IsActive = input.IsActive;
        account.IsDefault = input.IsDefault; account.UseForMarketData = input.UseForMarketData;
        account.SessionExpiresAt = input.SessionExpiresAt;
        account.ProtectedCredentials = protector.Protect(JsonSerializer.Serialize(secrets));
        if (!id.HasValue) db.Add(account);
        await db.SaveChangesAsync(ct); await transaction.CommitAsync(ct);
        await audit.WriteAsync("Broker", "BrokerConfiguration", account.BrokerAccountId.ToString(), "Updated", null,
            $"{account.ProviderCode}/{account.Environment}/{account.AccountReference}; credentials redacted", ct);
        return account;
    }
    private static string? Keep(string? value, string? previous) => string.IsNullOrWhiteSpace(value) ? previous : value.Trim();
}
