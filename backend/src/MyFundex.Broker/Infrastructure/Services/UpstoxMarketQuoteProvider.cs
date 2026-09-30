using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MyFundex.BuildingBlocks.Abstractions;
using MyFundex.BuildingBlocks.Domain;
using MyFundex.BuildingBlocks.Persistence;
using MyFundex.Contracts;

namespace MyFundex.Broker;

public sealed class UpstoxMarketQuoteProvider(HttpClient http, IRuntimeSettings settings)
    : IMarketQuoteProvider
{
    public async Task<decimal?> GetLtpAsync(string instrumentToken, CancellationToken ct)
    {
        var token =
            await settings.GetAsync("Broker.Upstox.MarketData.AccessToken", "GLOBAL", ct)
            ?? await settings.GetAsync("Broker.Upstox.Production.AccessToken", "GLOBAL", ct);
        if (string.IsNullOrWhiteSpace(token))
            return null;
        using var req = new HttpRequestMessage(
            HttpMethod.Get,
            "https://api.upstox.com/v3/market-quote/ltp?instrument_key="
                + Uri.EscapeDataString(instrumentToken)
        );
        req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        req.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        using var resp = await http.SendAsync(req, ct);
        if (!resp.IsSuccessStatusCode)
            return null;
        using var doc = JsonDocument.Parse(await resp.Content.ReadAsStringAsync(ct));
        if (
            !doc.RootElement.TryGetProperty("data", out var data)
            || data.ValueKind != JsonValueKind.Object
        )
            return null;
        foreach (var item in data.EnumerateObject())
        {
            if (
                item.Value.TryGetProperty("last_price", out var lp)
                && lp.TryGetDecimal(out var price)
            )
                return price;
        }
        return null;
    }
}
