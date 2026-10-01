using Microsoft.EntityFrameworkCore;
using MyFundex.Contracts;

namespace MyFundex.Trading;

public sealed class LiveRiskMonitor(
    TradingDbContext db,
    IMarketQuoteProvider quotes,
    ILiveRiskTermsReader terms
)
{
    public async Task CheckAsync(Guid accountId, CancellationToken ct)
    {
        var book = await db.Set<LiveBook>().SingleOrDefaultAsync(x => x.AccountId == accountId, ct);
        if (book == null || book.CloseRequestId != null)
            return;
        var rules =
            await terms.GetByAccountAsync(accountId, ct)
            ?? throw new ArgumentException("Funded risk terms are unavailable.");
        if (rules.DailyLossPercent <= 0 || rules.TotalLossPercent <= 0)
            throw new ArgumentException("Invalid funded loss limits.");
        var positions = await db.Set<LivePosition>()
            .Where(x => x.AccountId == accountId && x.Quantity > 0)
            .ToListAsync(ct);
        var prices = await quotes.GetLtpsAsync(positions.Select(x => x.InstrumentToken), ct);
        foreach (var position in positions)
            position.LastPrice =
                prices.TryGetValue(position.InstrumentToken, out var price) && price > 0
                    ? price
                    : throw new ArgumentException(
                        "Live valuation unavailable; new exposure is blocked."
                    );
        var today = DateOnly.FromDateTime(
            DateTimeOffset.UtcNow.ToOffset(TimeSpan.FromMinutes(330)).DateTime
        );
        if (book.TradingDate != today)
        {
            book.DayOpeningEquity = book.ValuedAt == null ? rules.Capital : book.Equity;
            book.TradingDate = today;
        }
        book.Equity = book.Cash + positions.Sum(x => x.Quantity * x.LastPrice);
        book.ValuedAt = DateTimeOffset.UtcNow;
        if (
            rules.Capital - book.Equity >= rules.Capital * rules.TotalLossPercent / 100m
            || book.DayOpeningEquity - book.Equity
                >= book.DayOpeningEquity * rules.DailyLossPercent / 100m
        )
        {
            book.Status = "Closing";
            book.CloseRequestId = Guid.NewGuid();
            book.CloseReason = "FundedLossLimit";
        }
        await db.SaveChangesAsync(ct);
    }
}
