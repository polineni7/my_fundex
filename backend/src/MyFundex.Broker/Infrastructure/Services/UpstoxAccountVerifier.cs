using System.Net.Http.Headers;
using System.Text.Json;
using MyFundex.Contracts;

namespace MyFundex.Broker;

public sealed class UpstoxAccountVerifier(HttpClient http, IRuntimeSettings settings)
    : IBrokerVerificationAdapter
{
    public string Provider => "Upstox";

    public async Task<bool> VerifyAsync(
        string provider,
        string credentialKey,
        string brokerUserId,
        CancellationToken ct
    )
    {
        if (
            provider != "Upstox"
            || string.IsNullOrWhiteSpace(credentialKey)
            || credentialKey.Length > 100
            || credentialKey.Any(c => !char.IsAsciiLetterOrDigit(c) && c != '-' && c != '_')
        )
            return false;
        var token = await settings.GetAsync(
            $"Broker.Upstox.Accounts.{credentialKey}.AccessToken",
            "GLOBAL",
            ct
        );
        if (string.IsNullOrWhiteSpace(token))
            return false;
        using var request = new HttpRequestMessage(
            HttpMethod.Get,
            "https://api.upstox.com/v2/user/profile"
        );
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        using var response = await http.SendAsync(request, ct);
        if (!response.IsSuccessStatusCode)
            return false;
        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync(ct));
        var data = json.RootElement.GetProperty("data");
        return data.GetProperty("is_active").GetBoolean()
            && data.GetProperty("user_id").GetString() == brokerUserId;
    }
}
