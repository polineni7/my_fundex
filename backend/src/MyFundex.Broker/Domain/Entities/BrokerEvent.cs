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

public sealed class BrokerEvent : EntityBase
{
    public Guid BrokerEventId { get; set; }
    public string Provider { get; set; } = "UPSTOX";
    public string Environment { get; set; } = "SANDBOX";
    public string EventType { get; set; } = "";
    public string? ProviderOrderId { get; set; }
    public string Payload { get; set; } = "";
    public DateTimeOffset ReceivedAt { get; set; } = DateTimeOffset.UtcNow;
}
