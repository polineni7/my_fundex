using Microsoft.EntityFrameworkCore;
using MyFundex.BuildingBlocks.Abstractions;
using MyFundex.BuildingBlocks.Results;
using MyFundex.Contracts;

namespace MyFundex.Trading;

public sealed class CancelOrderService(
    TradingDbContext db,
    IFundedAccountReader accounts,
    IOrderCancellationGateway broker,
    ICurrentActor actor,
    PaperTradingEngine? paper = null
)
{
    public async Task<Result<PlaceOrderResult>> CancelAsync(Guid orderId, CancellationToken ct)
    {
        var order = await db.Orders.SingleOrDefaultAsync(x => x.OrderId == orderId, ct);
        if (order == null)
            return Result<PlaceOrderResult>.Fail("Order not found.");
        var account = await accounts.GetByPublicIdAsync(order.AccountId, ct);
        if (account == null || actor.ActorId <= 0 || account.UserId != actor.ActorId)
            return Result<PlaceOrderResult>.Fail("Order not found.");
        if (order.BrokerProvider == "Paper")
        {
            if (paper == null)
                return Result<PlaceOrderResult>.Fail("Paper trading is unavailable.");
            await paper.CancelAsync(orderId, ct);
            return Result<PlaceOrderResult>.Ok(new(order.OrderId, order.Status, null));
        }
        if (
            order.Status
            is "Cancelled"
                or "CancellationRequested"
                or "CancellationPendingReconciliation"
        )
            return Result<PlaceOrderResult>.Ok(
                new(order.OrderId, order.Status, order.BrokerOrderId)
            );
        if (order.Status is not ("Submitted" or "PartiallyFilled") || order.BrokerOrderId == null)
            return Result<PlaceOrderResult>.Fail("This order cannot currently be cancelled.");

        var previous = order.Status;
        order.Status = "CancellationRequested";
        // Optimistic concurrency claims this cancellation before the external request.
        await db.SaveChangesAsync(ct);
        try
        {
            await broker.CancelAsync(
                new(
                    order.BrokerProvider,
                    order.BrokerEnvironment,
                    order.BrokerCredentialKey,
                    order.BrokerOrderId
                ),
                ct
            );
        }
        catch (Exception exception)
            when (exception
                    is HttpRequestException
                        or OperationCanceledException
                        or System.Text.Json.JsonException
            )
        {
            // A transport failure cannot establish whether the broker cancelled or filled the order.
        }
        order.Status = "CancellationPendingReconciliation";
        db.OrderEvents.Add(
            new OrderEvent
            {
                OrderEventId = Guid.NewGuid(),
                OrderInternalId = order.Id,
                EventType = "CancellationRequested",
                OldStatus = previous,
                NewStatus = order.Status,
            }
        );
        // Only reconciled fills and terminal broker status may release the reservation.
        await db.SaveChangesAsync(CancellationToken.None);
        return Result<PlaceOrderResult>.Ok(new(order.OrderId, order.Status, order.BrokerOrderId));
    }
}
