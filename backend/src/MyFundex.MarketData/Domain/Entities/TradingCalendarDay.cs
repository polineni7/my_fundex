using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MyFundex.BuildingBlocks.Abstractions;
using MyFundex.BuildingBlocks.Domain;
using MyFundex.BuildingBlocks.Persistence;

namespace MyFundex.MarketData;

public sealed class TradingCalendarDay : EntityBase
{
    public Guid CalendarDayId { get; set; }
    public DateOnly TradeDate { get; set; }
    public string ExchangeCode { get; set; } = "NSE";
    public bool IsTradingDay { get; set; }
    public TimeOnly? SessionOpen { get; set; }
    public TimeOnly? SessionClose { get; set; }
    public string? HolidayName { get; set; }
}
