using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MyFundex.BuildingBlocks.Abstractions;
using MyFundex.BuildingBlocks.Domain;
using MyFundex.BuildingBlocks.Persistence;

namespace MyFundex.Subscription;

public sealed class UserSubscription : EntityBase
{
    public Guid SubscriptionId { get; set; }
    public long UserInternalId { get; set; }
    public long PlanInternalId { get; set; }
    public long PlanVersionInternalId { get; set; }
    public string Status { get; set; } = "PendingPayment";
    public DateTimeOffset SubscribedAt { get; set; }
    public DateTimeOffset? ActivatedAt { get; set; }
    public DateTimeOffset? CompletedAt { get; set; }
    public DateTimeOffset? ExpiredAt { get; set; }
}
