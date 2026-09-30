using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MyFundex.BuildingBlocks.Abstractions;
using MyFundex.BuildingBlocks.Domain;
using MyFundex.BuildingBlocks.Persistence;
using MyFundex.BuildingBlocks.Results;
using MyFundex.Contracts;

namespace MyFundex.Trading;

public sealed class OrderEvent : EntityBase, IImmutableRecord
{
    public Guid OrderEventId { get; set; }
    public long OrderInternalId { get; set; }
    public string EventType { get; set; } = "";
    public string? OldStatus { get; set; }
    public string NewStatus { get; set; } = "";
    public string? ProviderPayload { get; set; }
    public DateTimeOffset OccurredAt { get; set; } = DateTimeOffset.UtcNow;
}
