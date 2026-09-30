using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MyFundex.BuildingBlocks.Abstractions;
using MyFundex.BuildingBlocks.Domain;
using MyFundex.BuildingBlocks.Persistence;
using MyFundex.BuildingBlocks.Results;
using MyFundex.Contracts;

namespace MyFundex.Trading;

public sealed class Order : EntityBase
{
    public string BrokerEnvironment { get; set; } = "SANDBOX";
    public string? BrokerCredentialKey { get; set; }
    public Guid OrderId { get; set; }
    public Guid AccountId { get; set; }
    public long FundedAccountInternalId { get; set; }
    public string InstrumentToken { get; set; } = "";
    public string Symbol { get; set; } = "";
    public string Side { get; set; } = "BUY";
    public string OrderType { get; set; } = "MARKET";
    public decimal Quantity { get; set; }
    public decimal? RequestedPrice { get; set; }
    public decimal EstimatedPrice { get; set; }
    public decimal FilledQuantity { get; set; }
    public decimal? AverageFillPrice { get; set; }
    public string Status { get; set; } = "Requested";
    public string? BrokerOrderId { get; set; }
    public Guid IdempotencyKey { get; set; }
    public Guid CorrelationId { get; set; }
    public DateTimeOffset? PlacedAt { get; set; }
    public DateTimeOffset? CompletedAt { get; set; }
}
