using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using MyFundex.Contracts;

namespace MyFundex.Trading;

public sealed class SquareOffService(
    TradingDbContext db,
    IOrderCancellationGateway cancellations,
    OrderService orders,
    IMarketQuoteProvider quotes,
    IInstrumentCatalogue instruments
)
{
    public async Task RequestAsync(Guid accountId, string reason, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(reason) || reason.Length > 200)
            throw new ArgumentException("A square-off reason is required.");
        var book = await db.Set<LiveBook>().SingleAsync(x => x.AccountId == accountId, ct);
        if (book.CloseRequestId != null)
            return;
        book.CloseRequestId = Guid.NewGuid();
        book.CloseReason = reason;
        book.Status = "Closing";
        await db.SaveChangesAsync(ct);
    }

    public async Task ProcessAsync(Guid accountId, CancellationToken ct)
    {
        var book = await db.Set<LiveBook>().SingleAsync(x => x.AccountId == accountId, ct);
        if (book.CloseRequestId == null || book.Status != "Closing")
            return;
        var pending = await db
            .Orders.Where(x =>
                x.AccountId == accountId
                && x.Status != "Filled"
                && x.Status != "Cancelled"
                && x.Status != "Rejected"
            )
            .ToListAsync(ct);
        if (pending.Count > 0)
        {
            foreach (
                var order in pending.Where(x =>
                    !x.IsLiquidation
                    && x.BrokerOrderId != null
                    && x.Status is "Submitted" or "PartiallyFilled"
                )
            )
            {
                order.Status = "CancellationRequested";
                await db.SaveChangesAsync(ct);
                await cancellations.CancelAsync(
                    new(
                        order.BrokerProvider,
                        order.BrokerEnvironment,
                        order.BrokerCredentialKey,
                        order.BrokerOrderId!
                    ),
                    ct
                );
                order.Status = "CancellationPendingReconciliation";
                await db.SaveChangesAsync(ct);
            }
            return;
        }
        var positions = await db.Set<LivePosition>()
            .AsNoTracking()
            .Where(x => x.AccountId == accountId && x.Quantity > 0)
            .ToListAsync(ct);
        if (positions.Count == 0)
        {
            book.Status = "Closed";
            await db.SaveChangesAsync(ct);
            return;
        }
        foreach (var position in positions)
        {
            var instrument = await instruments.GetAsync(position.InstrumentToken, ct);
            if (
                instrument == null
                || instrument.TickSize <= 0
                || !await instruments.IsSessionOpenAsync(instrument.Exchange, ct)
            )
                continue;
            var price = await quotes.GetLtpAsync(position.InstrumentToken, ct);
            if (price is null or <= 0)
                continue;
            var reference = new Guid(
                SHA256
                    .HashData(
                        Encoding.UTF8.GetBytes(book.CloseRequestId + ":" + position.InstrumentToken)
                    )
                    .AsSpan(0, 16)
            );
            var result = await orders.LiquidateAsync(
                new(
                    accountId,
                    position.InstrumentToken,
                    position.Symbol,
                    "SELL",
                    "LIMIT",
                    position.Quantity,
                    decimal.Floor(price.Value / instrument.TickSize) * instrument.TickSize,
                    reference
                ),
                ct
            );
            if (!result.Success)
                throw new ArgumentException(result.Error);
        }
    }
}
