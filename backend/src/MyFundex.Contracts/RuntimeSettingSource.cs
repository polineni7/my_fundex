namespace MyFundex.Contracts;

public interface IRuntimeSettingSource
{
    bool Handles(string key);
    Task<string?> GetAsync(string key, CancellationToken ct);
}
