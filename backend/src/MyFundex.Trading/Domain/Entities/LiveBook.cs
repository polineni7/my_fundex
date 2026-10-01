using MyFundex.BuildingBlocks.Domain;

namespace MyFundex.Trading;

public sealed class LiveBook : EntityBase
{
    public Guid AccountId { get; set; }
    public decimal Equity { get; set; }
    public decimal DayOpeningEquity { get; set; }
    public DateOnly TradingDate { get; set; }
    public DateTimeOffset? ValuedAt { get; set; }
    public Guid? CloseRequestId { get; set; }
    public string? CloseReason { get; set; }
    public decimal Cash { get; set; }
    public decimal ReservedCash { get; set; }
    public decimal RealizedProfit { get; set; }
    public string Status { get; set; } = "Active";
}

public sealed class LivePosition : EntityBase
{
    public Guid PositionId { get; set; } = Guid.NewGuid();
    public Guid AccountId { get; set; }
    public string InstrumentToken { get; set; } = "";
    public string Symbol { get; set; } = "";
    public decimal Quantity { get; set; }
    public decimal ReservedQuantity { get; set; }
    public decimal AverageCost { get; set; }
    public decimal LastPrice { get; set; }
    public decimal RealizedPnl { get; set; }
}
