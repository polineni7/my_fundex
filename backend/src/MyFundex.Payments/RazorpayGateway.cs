using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;

namespace MyFundex.Payments;

public sealed record RazorpayOrder(string Id,long Amount,string Currency,string KeyId);
public sealed record VerifiedPayment(string Id,string OrderId,long Amount,string Currency,string Status);

public static class PaymentSignature
{
    public static bool Verify(string payload,string? signature,string secret)
    {
        if(string.IsNullOrWhiteSpace(signature) || signature.Length!=64 || string.IsNullOrEmpty(secret)) return false;
        try
        {
            var expected=HMACSHA256.HashData(Encoding.UTF8.GetBytes(secret),Encoding.UTF8.GetBytes(payload));
            return CryptographicOperations.FixedTimeEquals(expected,Convert.FromHexString(signature));
        }
        catch(FormatException) { return false; }
    }
    public static long ToPaise(decimal amount)
    {
        if(amount<1 || decimal.Round(amount,2)!=amount) throw new ArgumentException("Payment amount must be at least INR 1 and have at most two decimal places.");
        return checked((long)(amount*100));
    }
}

public sealed class RazorpayGateway(HttpClient http,IConfiguration configuration)
{
    public bool IsConfigured => !string.IsNullOrWhiteSpace(configuration["Razorpay:KeyId"]) && !string.IsNullOrWhiteSpace(configuration["Razorpay:KeySecret"]);
    public string KeyId => configuration["Razorpay:KeyId"] ?? "";
    private HttpRequestMessage Request(HttpMethod method,string path)
    {
        if(!IsConfigured) throw new InvalidOperationException("Razorpay credentials are not configured.");
        var request=new HttpRequestMessage(method,"https://api.razorpay.com/v1/"+path);
        request.Headers.Authorization=new AuthenticationHeaderValue("Basic",Convert.ToBase64String(Encoding.UTF8.GetBytes($"{KeyId}:{configuration["Razorpay:KeySecret"]}")));
        return request;
    }
    public async Task<RazorpayOrder> CreateOrderAsync(Guid paymentId,decimal amount,CancellationToken ct)
    {
        using var request=Request(HttpMethod.Post,"orders");
        var paise=PaymentSignature.ToPaise(amount);
        request.Content=JsonContent.Create(new {amount=paise,currency="INR",receipt=paymentId.ToString("N"),partial_payment=false});
        using var response=await http.SendAsync(request,ct);
        response.EnsureSuccessStatusCode();
        using var json=JsonDocument.Parse(await response.Content.ReadAsStringAsync(ct));
        var root=json.RootElement;
        if(root.GetProperty("amount").GetInt64()!=paise || root.GetProperty("currency").GetString()!="INR") throw new InvalidOperationException("Unexpected provider order amount.");
        return new(root.GetProperty("id").GetString()!,paise,"INR",KeyId);
    }
    public bool VerifyCheckout(string storedOrderId,string paymentId,string signature) =>
        PaymentSignature.Verify(storedOrderId+"|"+paymentId,signature,configuration["Razorpay:KeySecret"]??"");
    public bool VerifyWebhook(string rawBody,string? signature) =>
        PaymentSignature.Verify(rawBody,signature,configuration["Razorpay:WebhookSecret"]??"");
    public async Task<VerifiedPayment> FetchPaymentAsync(string paymentId,CancellationToken ct)
    {
        using var request=Request(HttpMethod.Get,"payments/"+Uri.EscapeDataString(paymentId));
        using var response=await http.SendAsync(request,ct);
        response.EnsureSuccessStatusCode();
        using var json=JsonDocument.Parse(await response.Content.ReadAsStringAsync(ct));var root=json.RootElement;
        return new(root.GetProperty("id").GetString()!,root.GetProperty("order_id").GetString()!,root.GetProperty("amount").GetInt64(),root.GetProperty("currency").GetString()!,root.GetProperty("status").GetString()!);
    }
}
