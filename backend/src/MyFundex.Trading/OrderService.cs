using MyFundex.BuildingBlocks.Abstractions;
using MyFundex.BuildingBlocks.Results;
using MyFundex.Contracts;
using Microsoft.EntityFrameworkCore;

namespace MyFundex.Trading;

public static class OrderValidation
{
    public static string? Validate(PlaceOrderCommand c)
    {
        if(c.AccountId == Guid.Empty || c.IdempotencyKey == Guid.Empty) return "Account and idempotency key are required.";
        if(string.IsNullOrWhiteSpace(c.InstrumentToken) || string.IsNullOrWhiteSpace(c.Symbol)) return "Select an instrument.";
        if(c.Side is not ("BUY" or "SELL")) return "Side must be BUY or SELL.";
        if(c.OrderType is not ("MARKET" or "LIMIT")) return "Order type must be MARKET or LIMIT.";
        if(c.Quantity <= 0 || c.Quantity > int.MaxValue || decimal.Truncate(c.Quantity) != c.Quantity) return "Quantity must be a positive whole number.";
        if(c.OrderType == "LIMIT" && (!c.Price.HasValue || c.Price <= 0)) return "A positive limit price is required.";
        if(c.OrderType == "MARKET" && c.Price.HasValue) return "Market orders must not include a price.";
        return null;
    }
}

public sealed class OrderService(TradingDbContext db, IFundedAccountReader accounts,
    IFundedAccountCapitalService capital, IRiskGuard risk, IBrokerOrderGateway broker,
    IMarketQuoteProvider quotes, ICurrentActor actor)
{
    public async Task<Result<PlaceOrderResult>> PlaceAsync(PlaceOrderCommand c, CancellationToken ct)
    {
        var error = OrderValidation.Validate(c);
        if(error != null) return Result<PlaceOrderResult>.Fail(error);
        var account = await accounts.GetByPublicIdAsync(c.AccountId, ct);
        if(account == null || actor.ActorId <= 0 || account.UserId != actor.ActorId)
            return Result<PlaceOrderResult>.Fail("Account not found.");
        var existing = await db.Orders.AsNoTracking().SingleOrDefaultAsync(x=>x.IdempotencyKey==c.IdempotencyKey,ct);
        if(existing != null)
        {
            if(existing.AccountId != c.AccountId || existing.InstrumentToken != c.InstrumentToken || existing.Side != c.Side || existing.OrderType != c.OrderType || existing.Quantity != c.Quantity || existing.RequestedPrice != c.Price)
                return Result<PlaceOrderResult>.Fail("Idempotency key was already used for a different order.");
            return Result<PlaceOrderResult>.Ok(new(existing.OrderId,existing.Status,existing.BrokerOrderId));
        }
        if(account.Status != "Active") return Result<PlaceOrderResult>.Fail("Funded account is not active.");
        // Until sell-side inventory reservations and execution reconciliation are implemented, fail closed.
        if(c.Side == "SELL") return Result<PlaceOrderResult>.Fail("Selling requires inventory reservation and reconciliation, which are not enabled yet.");
        var quote = await quotes.GetLtpAsync(c.InstrumentToken,ct);
        if(quote is null or <= 0) return Result<PlaceOrderResult>.Fail("Current market price is unavailable.");
        var price = c.OrderType == "LIMIT" ? c.Price!.Value : quote.Value;
        var value = checked(c.Quantity * price);
        var check = await risk.EvaluateAsync(new(account.InternalId,account.AccountId,c.InstrumentToken,c.Side,c.Quantity,price,value),ct);
        if(!check.Allowed) return Result<PlaceOrderResult>.Fail(check.Message ?? "Risk rejected the order.");
        var order = new Order { OrderId=Guid.NewGuid(), AccountId=account.AccountId, FundedAccountInternalId=account.InternalId,
            InstrumentToken=c.InstrumentToken, Symbol=c.Symbol, Side=c.Side, OrderType=c.OrderType, Quantity=c.Quantity,
            EstimatedPrice=price, RequestedPrice=c.Price, IdempotencyKey=c.IdempotencyKey, CorrelationId=Guid.NewGuid(), Status="PendingReservation" };
        // Persist intent before moving capital. A crash leaves an observable intent for reconciliation.
        db.Add(order);
        await db.SaveChangesAsync(ct);
        if(!await capital.TryReserveAsync(account.InternalId,value,account.Version,ct))
        {
            order.Status="RiskRejected";
            await db.SaveChangesAsync(ct);
            return Result<PlaceOrderResult>.Fail("Buying power changed or is insufficient. Refresh and retry.");
        }
        order.Status="PendingSubmission";
        await db.SaveChangesAsync(ct);
        BrokerOrderResponse response;
        try { response=await broker.PlaceAsync(new(c.InstrumentToken,c.Side,c.Quantity,c.OrderType,c.Price,order.OrderId.ToString("N")),ct); }
        catch(Exception ex) when(ex is HttpRequestException or OperationCanceledException)
        {
            // A timeout does not prove rejection. Keep the reservation; never resubmit blindly.
            order.Status="ReconciliationRequired";
            await db.SaveChangesAsync(CancellationToken.None);
            return Result<PlaceOrderResult>.Ok(new(order.OrderId,order.Status,null));
        }
        if(!response.Success)
        {
            // Releasing capital and persisting order state span module boundaries. Defer to reconciliation
            // instead of risking a double release on retries or a crash between writes.
            order.Status="ReconciliationRequired";
        }
        else
        {
            order.Status="Submitted";
            order.BrokerOrderId=response.BrokerOrderId;
            order.PlacedAt=DateTimeOffset.UtcNow;
        }
        db.Add(new OrderEvent { OrderEventId=Guid.NewGuid(),OrderInternalId=order.Id,EventType=order.Status,
            OldStatus="PendingSubmission",NewStatus=order.Status,ProviderPayload=response.RawPayload });
        await db.SaveChangesAsync(CancellationToken.None);
        return Result<PlaceOrderResult>.Ok(new(order.OrderId,order.Status,order.BrokerOrderId));
    }
}
