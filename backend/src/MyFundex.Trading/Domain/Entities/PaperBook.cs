using MyFundex.BuildingBlocks.Domain;

namespace MyFundex.Trading;

public sealed class PaperBook : EntityBase
{
    public long AccountInternalId { get; set; }
    public Guid AccountId { get; set; }
    public decimal ReservedCash { get; set; }
    public decimal Cash { get; set; }
    public decimal Equity { get; set; }
    public decimal RealizedProfit { get; set; }
    public decimal DayOpeningEquity { get; set; }
    public DateOnly TradingDate { get; set; }
    public int TradingDays { get; set; }
    public DateOnly? LastTradeDate { get; set; }
    public string Status { get; set; } = "Active";
    public DateTimeOffset ValuedAt { get; set; }
}

public sealed class PaperPosition : EntityBase
{
    public Guid PositionId { get; set; } = Guid.NewGuid();
    public Guid AccountId { get; set; }
    public string InstrumentToken { get; set; } = "";
    public string Symbol { get; set; } = "";
    public decimal ReservedQuantity { get; set; }
    public decimal Quantity { get; set; }
    public decimal AverageCost { get; set; }
    public decimal LastPrice { get; set; }
    public decimal RealizedPnl { get; set; }
}

public static class PaperAccounting
{
    public static (decimal Cash, decimal Quantity, decimal AverageCost, decimal Profit) Fill(
        decimal cash,
        decimal held,
        decimal average,
        string side,
        decimal quantity,
        decimal price
    )
    {
        if (quantity <= 0 || decimal.Truncate(quantity) != quantity || price <= 0)
            throw new ArgumentException("Invalid paper fill.");
        var value = checked(quantity * price);
        if (side == "BUY")
        {
            if (value > cash)
                throw new ArgumentException("Insufficient buying power.");
            return (cash - value, held + quantity, (held * average + value) / (held + quantity), 0);
        }
        if (side != "SELL" || quantity > held)
            throw new ArgumentException("Insufficient equity inventory.");
        return (
            cash + value,
            held - quantity,
            held == quantity ? 0 : average,
            quantity * (price - average)
        );
    }
}
