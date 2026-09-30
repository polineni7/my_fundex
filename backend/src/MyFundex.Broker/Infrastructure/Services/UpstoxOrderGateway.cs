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

public sealed class UpstoxOrderGateway(HttpClient http, IRuntimeSettings settings)
    : IBrokerOrderGateway
{
    public async Task<BrokerOrderResponse> PlaceAsync(BrokerOrderRequest r, CancellationToken ct)
    {
        var env = r.Environment;
        if (env is not ("SANDBOX" or "PRODUCTION"))
            throw new ArgumentException("Invalid broker environment.");
        if (env == "PRODUCTION" && string.IsNullOrWhiteSpace(r.CredentialKey))
            return new(
                false,
                null,
                "ACCOUNT_NOT_LINKED",
                "A live broker account must be linked before trading.",
                ""
            );
        var key =
            env == "PRODUCTION"
                ? $"Broker.Upstox.Accounts.{r.CredentialKey}.AccessToken"
                : "Broker.Upstox.Sandbox.AccessToken";
        var token = await settings.GetAsync(key, "GLOBAL", ct);
        if (string.IsNullOrWhiteSpace(token))
            return new(false, null, "TOKEN_MISSING", "Upstox access token is not configured.", "");
        var url =
            env == "PRODUCTION"
                ? "https://api-hft.upstox.com/v3/order/place"
                : "https://sandbox.upstox.com/v3/order/place";
        using var req = new HttpRequestMessage(HttpMethod.Post, url);
        req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        req.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        req.Content = JsonContent.Create(
            new
            {
                quantity = Convert.ToInt32(r.Quantity),
                product = "D",
                validity = "DAY",
                price = r.Price ?? 0m,
                tag = r.Tag,
                instrument_token = r.InstrumentToken,
                order_type = r.OrderType,
                transaction_type = r.Side,
                disclosed_quantity = 0,
                trigger_price = 0m,
                is_amo = false,
                slice = false,
            }
        );
        using var resp = await http.SendAsync(req, ct);
        var raw = await resp.Content.ReadAsStringAsync(ct);
        string? oid = null;
        try
        {
            using var doc = JsonDocument.Parse(raw);
            if (
                doc.RootElement.TryGetProperty("data", out var d)
                && d.TryGetProperty("order_ids", out var o)
                && o.ValueKind == JsonValueKind.Array
                && o.GetArrayLength() == 1
            )
                oid = o[0].GetString();
        }
        catch { }
        return resp.IsSuccessStatusCode && !string.IsNullOrWhiteSpace(oid)
            ? new(true, oid, null, null, raw)
            : new(false, null, resp.StatusCode.ToString(), raw, raw);
    }
}
