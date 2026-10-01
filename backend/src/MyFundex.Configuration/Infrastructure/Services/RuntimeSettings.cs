using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using MyFundex.Contracts;

namespace MyFundex.Configuration;

public sealed class RuntimeSettings(
    ConfigurationDbContext db,
    IMemoryCache cache,
    IConfiguration configuration,
    IAuditWriter audit,
    IEnumerable<IRuntimeSettingSource> sources
) : IRuntimeSettings
{
    public static bool IsSecret(string key) =>
        new[] { "token", "secret", "password", "privatekey", "signingkey" }.Any(part =>
            key.Contains(part, StringComparison.OrdinalIgnoreCase)
        );

    public async Task<string?> GetAsync(string key, string env, CancellationToken ct)
    {
        var source = sources.FirstOrDefault(x => x.Handles(key));
        if (source != null) return await source.GetAsync(key, ct);
        if (IsSecret(key))
            return configuration[key.Replace('.', ':')];
        var cacheKey = $"cfg:{env}:{key}";
        if (cache.TryGetValue<string>(cacheKey, out var value))
            return value;
        value = await db
            .Settings.AsNoTracking()
            .Where(x => x.SettingKey == key && x.Environment == env)
            .Select(x => x.SettingValue)
            .SingleOrDefaultAsync(ct);
        if (value != null)
            cache.Set(cacheKey, value, TimeSpan.FromSeconds(30));
        return value;
    }

    public async Task UpsertAsync(
        string key,
        string value,
        string env,
        string category,
        long actor,
        CancellationToken ct
    )
    {
        if (
            string.IsNullOrWhiteSpace(key)
            || key.Length > 200
            || value.Length > 4000
            || env.Length > 50
            || category.Length > 100
        )
            throw new ArgumentException("Invalid setting.");
        if (key.StartsWith("Broker.", StringComparison.OrdinalIgnoreCase))
            throw new ArgumentException("Use the Broker setup form to manage broker connections.");
        if (IsSecret(key))
            throw new ArgumentException(
                "Secrets must be configured through environment variables or a secret store."
            );
        var setting = await db.Settings.SingleOrDefaultAsync(
            x => x.SettingKey == key && x.Environment == env,
            ct
        );
        var previous = setting?.SettingValue;
        if (setting == null)
        {
            setting = new()
            {
                SettingId = Guid.NewGuid(),
                SettingKey = key,
                SettingValue = value,
                Environment = env,
                Category = category,
            };
            db.Add(setting);
        }
        else
            setting.SettingValue = value;
        await db.SaveChangesAsync(ct);
        cache.Remove($"cfg:{env}:{key}");
        await audit.WriteAsync(
            "Configuration",
            "Setting",
            setting.SettingId.ToString(),
            "Updated",
            previous,
            value,
            ct
        );
    }
}
