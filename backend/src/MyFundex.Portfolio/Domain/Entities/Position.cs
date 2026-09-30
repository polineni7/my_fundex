using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MyFundex.BuildingBlocks.Abstractions;
using MyFundex.BuildingBlocks.Domain;
using MyFundex.BuildingBlocks.Persistence;

namespace MyFundex.Portfolio;

public sealed class Position : EntityBase
{
    public Guid PositionId { get; set; }
    public long FundedAccountInternalId { get; set; }
    public string InstrumentToken { get; set; } = "";
    public string Symbol { get; set; } = "";
    public decimal Quantity { get; set; }
    public decimal AverageCost { get; set; }
    public decimal RealizedPnl { get; set; }
    public decimal UnrealizedPnl { get; set; }
    public string Status { get; set; } = "Open";
    public DateTimeOffset OpenedAt { get; set; }
    public DateTimeOffset? ClosedAt { get; set; }
}
