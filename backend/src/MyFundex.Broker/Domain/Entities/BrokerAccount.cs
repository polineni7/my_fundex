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

public sealed class BrokerAccount : EntityBase
{
    public Guid BrokerAccountId { get; set; }
    public string ProviderCode { get; set; } = "UPSTOX";
    public string Environment { get; set; } = "SANDBOX";
    public string AccountReference { get; set; } = "";
    public string DisplayName { get; set; } = "";
    public string BrokerUserId { get; set; } = "";
    public string? ProtectedCredentials { get; set; }
    public DateTimeOffset? SessionExpiresAt { get; set; }
    public bool UseForMarketData { get; set; }
    public bool IsDefault { get; set; }
    public bool IsActive { get; set; } = true;
}
