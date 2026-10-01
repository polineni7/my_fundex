using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using MyFundex.Contracts;

namespace MyFundex.Payments;

public sealed class RazorpayPayoutGateway(HttpClient http, IConfiguration configuration)
    : IPayoutGateway
{
    public async Task<PayoutResult> SendAsync(
        Guid withdrawalId,
        string beneficiary,
        decimal amount,
        CancellationToken ct
    )
    {
        using var request = Create(HttpMethod.Post, "/v1/payouts");
        request.Headers.Add("X-Payout-Idempotency", withdrawalId.ToString());
        request.Content = JsonContent.Create(
            new
            {
                account_number = configuration["RazorpayX:AccountNumber"]
                    ?? throw new ArgumentException("Payout account is not configured."),
                fund_account_id = beneficiary,
                amount = PaymentSignature.ToPaise(amount),
                currency = "INR",
                mode = "NEFT",
                purpose = "payout",
                queue_if_low_balance = false,
                reference_id = withdrawalId.ToString("N"),
                narration = "MyFundex reward",
            }
        );
        return await SendAsync(request, ct);
    }

    public async Task<PayoutResult> ReadAsync(string payoutId, CancellationToken ct)
    {
        using var request = Create(HttpMethod.Get, "/v1/payouts/" + Uri.EscapeDataString(payoutId));
        return await SendAsync(request, ct);
    }

    private HttpRequestMessage Create(HttpMethod method, string path)
    {
        if (!configuration.GetValue<bool>("RazorpayX:Enabled"))
            throw new ArgumentException("Payouts are disabled.");
        var key = configuration["RazorpayX:KeyId"];
        var secret = configuration["RazorpayX:KeySecret"];
        var prefix = configuration["RazorpayX:Mode"] switch
        {
            "Live" => "rzp_live_",
            "Test" => "rzp_test_",
            _ => throw new ArgumentException("Explicit payout mode is required."),
        };
        if (
            key == null
            || !key.StartsWith(prefix, StringComparison.Ordinal)
            || string.IsNullOrWhiteSpace(secret)
        )
            throw new ArgumentException("Payout credentials do not match the configured mode.");
        var request = new HttpRequestMessage(method, "https://api.razorpay.com" + path);
        request.Headers.Authorization = new AuthenticationHeaderValue(
            "Basic",
            Convert.ToBase64String(Encoding.UTF8.GetBytes(key + ":" + secret))
        );
        return request;
    }

    private async Task<PayoutResult> SendAsync(HttpRequestMessage request, CancellationToken ct)
    {
        using var response = await http.SendAsync(request, ct);
        response.EnsureSuccessStatusCode();
        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync(ct));
        var data = json.RootElement;
        if (data.GetProperty("currency").GetString() != "INR")
            throw new ArgumentException("Unexpected payout currency.");
        return new(
            data.GetProperty("id").GetString()!,
            data.GetProperty("status").GetString()!,
            data.GetProperty("amount").GetDecimal() / 100m,
            data.GetProperty("reference_id").GetString()!
        );
    }
}
