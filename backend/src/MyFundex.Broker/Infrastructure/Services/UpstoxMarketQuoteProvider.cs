using System.Globalization;
using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.Extensions.Caching.Memory;
using MyFundex.Contracts;

namespace MyFundex.Broker;

public sealed class UpstoxMarketQuoteProvider(
    HttpClient http,
    IRuntimeSettings settings,
    IMemoryCache cache
) : IMarketQuoteProvider
{
    public async Task<decimal?> GetLtpAsync(string instrumentToken, CancellationToken ct)
    {
        var prices = await GetLtpsAsync([instrumentToken], ct);
        return prices.TryGetValue(instrumentToken, out var price) ? price : null;
    }

    public async Task<IReadOnlyDictionary<string, decimal>> GetLtpsAsync(
        IEnumerable<string> instruments,
        CancellationToken ct
    )
    {
        var tokens = instruments.Distinct(StringComparer.Ordinal).ToArray();
        if (
            tokens.Length > 500
            || tokens.Any(x => string.IsNullOrWhiteSpace(x) || x.Length > 100 || x.Contains(','))
        )
            throw new ArgumentException("Invalid quote request.");
        var prices = new Dictionary<string, decimal>(StringComparer.Ordinal);
        var missing = new HashSet<string>(StringComparer.Ordinal);
        foreach (var token in tokens)
            if (cache.TryGetValue<decimal>("upstox:quote:" + token, out var price))
                prices[token] = price;
            else
                missing.Add(token);
        if (missing.Count == 0)
            return prices;
        var accessToken = await settings.GetAsync(
            "Broker.Upstox.MarketData.AccessToken",
            "GLOBAL",
            ct
        );
        if (string.IsNullOrWhiteSpace(accessToken))
            return prices;
        using var request = new HttpRequestMessage(
            HttpMethod.Get,
            "https://api.upstox.com/v2/market-quote/quotes?instrument_key="
                + Uri.EscapeDataString(string.Join(',', missing))
        );
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        using var response = await http.SendAsync(request, ct);
        if (!response.IsSuccessStatusCode)
            return prices;
        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync(ct));
        if (
            !json.RootElement.TryGetProperty("data", out var data)
            || data.ValueKind != JsonValueKind.Object
        )
            return prices;
        foreach (var item in data.EnumerateObject())
        {
            if (!item.Value.TryGetProperty("instrument_token", out var value))
                continue;
            var token = value.GetString();
            if (token == null || !missing.Contains(token))
                continue;
            var price = ReadQuote(item.Value, DateTimeOffset.UtcNow);
            if (price.HasValue)
            {
                prices[token] = price.Value;
                cache.Set("upstox:quote:" + token, price.Value, TimeSpan.FromSeconds(1));
            }
        }
        return prices;
    }

    public static decimal? ReadFreshPrice(JsonElement root, string instrument, DateTimeOffset now)
    {
        if (!root.TryGetProperty("data", out var data) || data.ValueKind != JsonValueKind.Object)
            return null;
        foreach (var item in data.EnumerateObject())
            if (
                item.Value.TryGetProperty("instrument_token", out var token)
                && token.GetString() == instrument
            )
                return ReadQuote(item.Value, now);
        return null;
    }

    private static decimal? ReadQuote(JsonElement quote, DateTimeOffset now)
    {
        if (
            !quote.TryGetProperty("last_trade_time", out var time)
            || !long.TryParse(
                time.ToString(),
                NumberStyles.Integer,
                CultureInfo.InvariantCulture,
                out var millis
            )
        )
            return null;
        DateTimeOffset tradedAt;
        try
        {
            tradedAt = DateTimeOffset.FromUnixTimeMilliseconds(millis);
        }
        catch (ArgumentOutOfRangeException)
        {
            return null;
        }
        if (tradedAt > now.AddSeconds(5) || tradedAt < now.AddMinutes(-2))
            return null;
        return
            quote.TryGetProperty("last_price", out var last)
            && last.TryGetDecimal(out var price)
            && price > 0
            ? decimal.Round(price, 4)
            : null;
    }
}
