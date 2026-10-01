using Microsoft.EntityFrameworkCore;
using MyFundex.Contracts;

namespace MyFundex.Trading;

public sealed class LiveSettlementService(
    TradingDbContext db,
    IBrokerOrderReader broker,
    IOrderCancellationGateway? cancellations = null,
    ILiveRiskTermsReader? profitTerms = null
)
{
    public async Task ReconcileAsync(Guid orderId, CancellationToken ct)
    {
        var order = await db.Orders.SingleAsync(x => x.OrderId == orderId, ct);
        if (
            !order.UsesLiveBook
            || order.BrokerEnvironment != "PRODUCTION"
            || order.Status is "Filled" or "Cancelled" or "Rejected"
        )
            return;
        var observation = await broker.ReadAsync(
            order.BrokerProvider,
            order.BrokerCredentialKey!,
            order.BrokerOrderId,
            order.OrderId.ToString("N"),
            ct
        );
        if (observation == null)
            return;
        if (
            observation.InstrumentToken != order.InstrumentToken
            || observation.Side != order.Side
            || observation.Quantity != order.Quantity
        )
            throw new ArgumentException("Broker order differs from the recorded intent.");
        if (
            observation.Fills.Sum(x => x.Quantity) != observation.FilledQuantity
            || observation.FilledQuantity > order.Quantity
            || observation.FilledQuantity < 0
        )
            throw new ArgumentException(
                "Broker fills are incomplete or inconsistent; settlement is deferred."
            );
        var allocationTerms =
            profitTerms == null ? null : await profitTerms.GetByAccountAsync(order.AccountId, ct);
        var book = await db.Set<LiveBook>().SingleAsync(x => x.AccountId == order.AccountId, ct);
        var position = await db.Set<LivePosition>()
            .SingleAsync(
                x => x.AccountId == order.AccountId && x.InstrumentToken == order.InstrumentToken,
                ct
            );
        var known = await db.Executions.Where(x => x.OrderInternalId == order.Id).ToListAsync(ct);
        var observedIds = observation
            .Fills.Select(x => x.TradeId)
            .ToHashSet(StringComparer.Ordinal);
        var knownById = known.ToDictionary(x => x.BrokerExecutionId!, StringComparer.Ordinal);
        if (
            observedIds.Count != observation.Fills.Count
            || known.Any(x => !observedIds.Contains(x.BrokerExecutionId!))
        )
            throw new ArgumentException("Broker execution history is incomplete or duplicated.");
        foreach (var fill in observation.Fills.OrderBy(x => x.ExecutedAt).ThenBy(x => x.TradeId))
        {
            if (
                string.IsNullOrWhiteSpace(fill.TradeId)
                || fill.Quantity <= 0
                || decimal.Truncate(fill.Quantity) != fill.Quantity
                || fill.Price <= 0
            )
                throw new ArgumentException("Invalid broker fill.");
            knownById.TryGetValue(fill.TradeId, out var prior);
            if (prior != null)
            {
                if (prior.Quantity != fill.Quantity || prior.Price != fill.Price)
                    throw new ArgumentException("Broker changed a settled execution.");
                continue;
            }
            var realized =
                order.Side == "SELL"
                    ? fill.Quantity * (fill.Price - position.AverageCost)
                    : (decimal?)null;
            var value = fill.Quantity * fill.Price;
            position.LastPrice = fill.Price;
            if (order.Side == "BUY")
            {
                var reserved = Math.Min(order.ReservedCash, fill.Quantity * order.EstimatedPrice);
                book.ReservedCash -= reserved;
                order.ReservedCash -= reserved;
                position.AverageCost =
                    (position.Quantity * position.AverageCost + value)
                    / (position.Quantity + fill.Quantity);
                position.Quantity += fill.Quantity;
                book.Cash -= value;
            }
            else
            {
                if (position.Quantity < fill.Quantity || position.ReservedQuantity < fill.Quantity)
                    throw new ArgumentException("Broker sell exceeds reserved inventory.");
                var profit = fill.Quantity * (fill.Price - position.AverageCost);
                position.Quantity -= fill.Quantity;
                position.ReservedQuantity -= fill.Quantity;
                position.RealizedPnl += profit;
                book.RealizedProfit += profit;
                book.Cash += value;
                if (position.Quantity == 0)
                    position.AverageCost = 0;
            }
            db.Executions.Add(
                new Execution
                {
                    RealizedProfit = realized,
                    TraderSharePercent = allocationTerms?.RewardSharePercent,
                    TaxWithholdingPercent = allocationTerms?.TaxWithholdingPercent,
                    OtherDeductionPercent = allocationTerms?.OtherDeductionPercent,
                    ExecutionId = Guid.NewGuid(),
                    OrderInternalId = order.Id,
                    BrokerExecutionId = fill.TradeId,
                    Quantity = fill.Quantity,
                    Price = fill.Price,
                    GrossValue = value,
                    ExecutedAt = fill.ExecutedAt,
                }
            );
        }
        if (book.Cash < book.ReservedCash)
            book.Status = "DeficitReview";
        var previous = order.Status;
        order.BrokerOrderId = observation.OrderId;
        order.FilledQuantity = observation.FilledQuantity;
        order.AverageFillPrice =
            observation.FilledQuantity == 0
                ? null
                : observation.Fills.Sum(x => x.Quantity * x.Price) / observation.FilledQuantity;
        var terminal = observation.Status is "complete" or "cancelled" or "rejected";
        if (terminal)
        {
            if (observation.Status == "complete" && observation.FilledQuantity != order.Quantity)
                throw new ArgumentException("Completed order has missing fills.");
            if (order.Side == "BUY")
            {
                book.ReservedCash -= order.ReservedCash;
                order.ReservedCash = 0;
            }
            else
                position.ReservedQuantity -= order.Quantity - order.FilledQuantity;
            order.Status = observation.Status switch
            {
                "complete" => "Filled",
                "cancelled" => "Cancelled",
                _ => "Rejected",
            };
            order.CompletedAt = DateTimeOffset.UtcNow;
        }
        else if (
            order.Status is not ("CancellationRequested" or "CancellationPendingReconciliation")
        )
            order.Status = observation.FilledQuantity > 0 ? "PartiallyFilled" : "Submitted";
        // Even a sell proceeds update participates in account-wide optimistic concurrency.
        db.Entry(book).Property(x => x.Cash).IsModified = true;
        if (previous != order.Status)
            db.OrderEvents.Add(
                new OrderEvent
                {
                    OrderEventId = Guid.NewGuid(),
                    OrderInternalId = order.Id,
                    EventType = "Reconciled",
                    OldStatus = previous,
                    NewStatus = order.Status,
                }
            );
        await db.SaveChangesAsync(ct);
        if (
            !terminal
            && cancellations != null
            && order.Status is "CancellationRequested" or "CancellationPendingReconciliation"
        )
        {
            // Retry only cancellation of the same verified broker order, never order submission.
            // A crash before/after this call is recoverable on the next reconciliation pass.
            await cancellations.CancelAsync(
                new(
                    order.BrokerProvider,
                    order.BrokerEnvironment,
                    order.BrokerCredentialKey,
                    observation.OrderId
                ),
                ct
            );
        }
    }
}
