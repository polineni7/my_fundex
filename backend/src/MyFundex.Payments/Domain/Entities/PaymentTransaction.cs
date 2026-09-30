using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MyFundex.BuildingBlocks.Abstractions;
using MyFundex.BuildingBlocks.Domain;
using MyFundex.BuildingBlocks.Persistence;

namespace MyFundex.Payments;

public sealed class PaymentTransaction : EntityBase
{
    public DateTimeOffset? FulfilledAt { get; set; }
    public Guid PaymentId { get; set; }
    public long UserInternalId { get; set; }
    public Guid? SubscriptionId { get; set; }
    public string Provider { get; set; } = "";
    public string? ProviderOrderId { get; set; }
    public Guid IdempotencyKey { get; set; }
    public string? ProviderPaymentId { get; set; }
    public decimal Amount { get; set; }
    public string CurrencyCode { get; set; } = "INR";
    public string Status { get; set; } = "Pending";
    public string Purpose { get; set; } = "Subscription";
    public DateTimeOffset? PaidAt { get; set; }
    public string? FailureReason { get; set; }
}
