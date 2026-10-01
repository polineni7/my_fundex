using Microsoft.EntityFrameworkCore;
using MyFundex.Contracts;

namespace MyFundex.MarketData;

public sealed class InstrumentCatalogue(MarketDataDbContext db) : IInstrumentCatalogue
{
    public Task<TradableInstrument?> GetAsync(string token, CancellationToken ct) =>
        db
            .Instruments.AsNoTracking()
            .Where(x => x.InstrumentToken == token && x.IsActive)
            .Select(x => new TradableInstrument(
                x.InstrumentToken,
                x.TradingSymbol,
                x.ExchangeCode,
                x.SecurityType,
                x.TickSize,
                x.LotSize
            ))
            .SingleOrDefaultAsync(ct);

    public async Task<bool> IsSessionOpenAsync(string exchange, CancellationToken ct)
    {
        var now = DateTimeOffset.UtcNow.ToOffset(TimeSpan.FromMinutes(330));
        var date = DateOnly.FromDateTime(now.DateTime);
        var time = TimeOnly.FromDateTime(now.DateTime);
        var day = await db
            .TradingCalendar.AsNoTracking()
            .SingleOrDefaultAsync(x => x.ExchangeCode == exchange && x.TradeDate == date, ct);
        return day is { IsTradingDay: true, SessionOpen: not null, SessionClose: not null }
            && time >= day.SessionOpen
            && time < day.SessionClose;
    }
}
