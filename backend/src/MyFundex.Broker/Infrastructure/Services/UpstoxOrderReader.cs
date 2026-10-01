using System.Globalization;
using System.Net.Http.Headers;
using System.Text.Json;
using MyFundex.Contracts;

namespace MyFundex.Broker;

public sealed class UpstoxOrderReader(HttpClient http, IRuntimeSettings settings)
    : IBrokerReconciliationAdapter
{
    public string Provider => "Upstox";

    public async Task<BrokerObservation?> ReadAsync(
        string provider,
        string credentialKey,
        string? orderId,
        string tag,
        CancellationToken ct
    )
    {
        if (provider != "Upstox")
            throw new ArgumentException("Provider reconciliation adapter is not configured.");
        var token = await settings.GetAsync(
            $"Broker.Upstox.Accounts.{credentialKey}.AccessToken",
            "GLOBAL",
            ct
        );
        if (string.IsNullOrWhiteSpace(token))
            throw new ArgumentException("Broker credentials unavailable.");
        using var details = await ReadAsync(
            "/v2/order/details?"
                + (
                    orderId == null
                        ? "tag=" + Uri.EscapeDataString(tag)
                        : "order_id=" + Uri.EscapeDataString(orderId)
                ),
            token,
            ct
        );
        if (details == null)
            return null;
        var data = details.RootElement.GetProperty("data");
        var id = data.GetProperty("order_id").GetString()!;
        if (orderId != null && id != orderId || data.GetProperty("tag").GetString() != tag)
            throw new ArgumentException("Broker order identity does not match the local intent.");
        using var trades = await ReadAsync(
            "/v2/order/trades?order_id=" + Uri.EscapeDataString(id),
            token,
            ct
        );
        if (trades == null)
            return null;
        var fills = new List<BrokerFill>();
        foreach (var trade in trades.RootElement.GetProperty("data").EnumerateArray())
        {
            if (trade.GetProperty("order_id").GetString() != id)
                throw new ArgumentException("Broker fill belongs to another order.");
            var rawTime = trade.GetProperty("exchange_timestamp").GetString();
            if (
                !DateTime.TryParseExact(
                    rawTime,
                    ["dd-MMM-yyyy HH:mm:ss", "yyyy-MM-dd HH:mm:ss"],
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out var time
                )
            )
                throw new ArgumentException("Unrecognized broker execution timestamp.");
            fills.Add(
                new(
                    trade.GetProperty("trade_id").GetString()!,
                    trade.GetProperty("quantity").GetDecimal(),
                    trade.GetProperty("average_price").GetDecimal(),
                    new DateTimeOffset(
                        DateTime.SpecifyKind(time, DateTimeKind.Unspecified),
                        TimeSpan.FromMinutes(330)
                    ).ToUniversalTime()
                )
            );
        }
        return new(
            id,
            data.GetProperty("status").GetString()!,
            data.GetProperty("instrument_token").GetString()!,
            data.GetProperty("transaction_type").GetString()!,
            data.GetProperty("quantity").GetDecimal(),
            data.GetProperty("filled_quantity").GetDecimal(),
            fills
        );
    }

    private async Task<JsonDocument?> ReadAsync(string path, string token, CancellationToken ct)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, "https://api.upstox.com" + path);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        using var response = await http.SendAsync(request, ct);
        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            return null;
        response.EnsureSuccessStatusCode();
        return JsonDocument.Parse(await response.Content.ReadAsStringAsync(ct));
    }
}
