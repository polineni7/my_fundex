using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MyFundex.BuildingBlocks.Abstractions;
using MyFundex.BuildingBlocks.Domain;
using MyFundex.BuildingBlocks.Persistence;

namespace MyFundex.MarketData;

public sealed class Instrument : EntityBase
{
    public Guid InstrumentId { get; set; }
    public string InstrumentToken { get; set; } = "";
    public string ExchangeCode { get; set; } = "NSE";
    public string Symbol { get; set; } = "";
    public string TradingSymbol { get; set; } = "";
    public string? Isin { get; set; }
    public string Name { get; set; } = "";
    public string SecurityType { get; set; } = "EQUITY";
    public decimal TickSize { get; set; }
    public int LotSize { get; set; } = 1;
    public bool IsActive { get; set; } = true;
}
