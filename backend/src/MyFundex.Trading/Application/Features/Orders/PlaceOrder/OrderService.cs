using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using MyFundex.BuildingBlocks.Abstractions;
using MyFundex.BuildingBlocks.Results;
using MyFundex.Contracts;

namespace MyFundex.Trading;

public static class OrderValidation
{
    public static string? Validate(PlaceOrderCommand c)
    {
        if (c.AccountId == Guid.Empty || c.IdempotencyKey == Guid.Empty)
            return "Account and idempotency key are required.";
        if (string.IsNullOrWhiteSpace(c.InstrumentToken) || string.IsNullOrWhiteSpace(c.Symbol))
            return "Select an instrument.";
        if (c.Side is not ("BUY" or "SELL"))
            return "Side must be BUY or SELL.";
        if (c.OrderType is not ("MARKET" or "LIMIT"))
            return "Order type must be MARKET or LIMIT.";
        if (
            c.Quantity <= 0
            || c.Quantity > int.MaxValue
            || decimal.Truncate(c.Quantity) != c.Quantity
        )
            return "Quantity must be a positive whole number.";
        if (c.OrderType == "LIMIT" && (!c.Price.HasValue || c.Price <= 0))
            return "A positive limit price is required.";
        if (c.OrderType == "MARKET" && c.Price.HasValue)
            return "Market orders must not include a price.";
        return null;
    }
}

public sealed class OrderService(
    TradingDbContext db,
    IFundedAccountReader accounts,
    IRiskGuard risk,
    IBrokerOrderGateway broker,
    IMarketQuoteProvider quotes,
    ICurrentActor actor,
    ITradingEligibility eligibility,
    IInstrumentCatalogue instruments,
    IConfiguration configuration,
    LiveRiskMonitor monitor,
    IRuntimeSettings? runtimeSettings = null
)
{
    public Task<Result<PlaceOrderResult>> PlaceAsync(PlaceOrderCommand c, CancellationToken ct) =>
        PlaceCoreAsync(c, false, ct);

    internal Task<Result<PlaceOrderResult>> LiquidateAsync(
        PlaceOrderCommand c,
        CancellationToken ct
    ) => PlaceCoreAsync(c, true, ct);

    private async Task<Result<PlaceOrderResult>> PlaceCoreAsync(
        PlaceOrderCommand c,
        bool liquidation,
        CancellationToken ct
    )
    {
        if (
            !string.Equals(
                runtimeSettings == null ? configuration["Trading:LiveEnabled"] : await runtimeSettings.GetAsync("Trading.LiveEnabled", "GLOBAL", ct),
                "true",
                StringComparison.OrdinalIgnoreCase
            )
        )
            return Result<PlaceOrderResult>.Fail("Real trading is disabled in admin runtime settings.");
        var error = OrderValidation.Validate(c);
        if (error != null)
            return Result<PlaceOrderResult>.Fail(error);
        var account = await accounts.GetByPublicIdAsync(c.AccountId, ct);
        if (
            account == null
            || !liquidation && (actor.ActorId <= 0 || account.UserId != actor.ActorId)
        )
            return Result<PlaceOrderResult>.Fail("Account not found.");
        var existing = await db
            .Orders.AsNoTracking()
            .SingleOrDefaultAsync(x => x.IdempotencyKey == c.IdempotencyKey, ct);
        if (existing != null)
        {
            if (
                existing.AccountId != c.AccountId
                || existing.InstrumentToken != c.InstrumentToken
                || existing.Side != c.Side
                || existing.OrderType != c.OrderType
                || existing.Quantity != c.Quantity
                || existing.RequestedPrice != c.Price
            )
                return Result<PlaceOrderResult>.Fail(
                    "Idempotency key was already used for a different order."
                );
            return Result<PlaceOrderResult>.Ok(
                new(existing.OrderId, existing.Status, existing.BrokerOrderId)
            );
        }
        if (!liquidation && account.Status != "Active")
            return Result<PlaceOrderResult>.Fail("Funded account is not active.");
        var access = await eligibility.CheckAsync(
            account.AccountId,
            account.UserId,
            account.TradingMode,
            ct
        );
        if (!liquidation && !access.Allowed)
            return Result<PlaceOrderResult>.Fail(access.Reason ?? "Trading is unavailable.");
        if (account.TradingMode != "Funded")
            return Result<PlaceOrderResult>.Fail(
                "Use the paper trading engine for evaluation orders."
            );
        // Live limit orders cap the maximum reserved principal. Unbounded market orders are not accepted.
        if (c.OrderType != "LIMIT")
            return Result<PlaceOrderResult>.Fail("Real trading currently requires a limit price.");
        var instrument = await instruments.GetAsync(c.InstrumentToken, ct);
        if (
            instrument == null
            || instrument.SecurityType != "EQUITY"
            || !await instruments.IsSessionOpenAsync(instrument.Exchange, ct)
        )
            return Result<PlaceOrderResult>.Fail(
                "Select an active equity during its configured trading session."
            );
        if (
            instrument.TickSize <= 0
            || c.Price!.Value % instrument.TickSize != 0
            || instrument.LotSize <= 0
            || c.Quantity % instrument.LotSize != 0
        )
            return Result<PlaceOrderResult>.Fail(
                "Order price or quantity does not match the instrument tick and lot size."
            );
        c = c with { Symbol = instrument.Symbol };
        var quote = await quotes.GetLtpAsync(c.InstrumentToken, ct);
        if (quote is null or <= 0)
            return Result<PlaceOrderResult>.Fail("Current market price is unavailable.");
        var price = c.OrderType == "LIMIT" ? c.Price!.Value : quote.Value;
        var value = checked(c.Quantity * price);
        var check = await risk.EvaluateAsync(
            new(
                account.InternalId,
                account.AccountId,
                c.InstrumentToken,
                c.Side,
                c.Quantity,
                price,
                value
            ),
            ct
        );
        if (!liquidation && !check.Allowed)
            return Result<PlaceOrderResult>.Fail(check.Message ?? "Risk rejected the order.");
        var order = new Order
        {
            OrderId = Guid.NewGuid(),
            IsLiquidation = liquidation,
            AccountId = account.AccountId,
            FundedAccountInternalId = account.InternalId,
            InstrumentToken = c.InstrumentToken,
            Symbol = c.Symbol,
            Side = c.Side,
            OrderType = c.OrderType,
            Quantity = c.Quantity,
            EstimatedPrice = price,
            RequestedPrice = c.Price,
            BrokerEnvironment = AccountTradingRoute.Resolve(account.TradingMode),
            BrokerCredentialKey = account.BrokerCredentialKey,
            BrokerProvider = account.BrokerProvider,
            IdempotencyKey = c.IdempotencyKey,
            CorrelationId = Guid.NewGuid(),
            Status = "PendingReservation",
        };
        if (!liquidation)
            await monitor.CheckAsync(account.AccountId, ct);
        var book = await db.Set<LiveBook>()
            .SingleOrDefaultAsync(x => x.AccountId == account.AccountId, ct);
        if (book == null)
        {
            if (await db.Orders.AnyAsync(x => x.AccountId == account.AccountId, ct))
                return Result<PlaceOrderResult>.Fail(
                    "Existing trading history requires ledger reconciliation."
                );
            book = new LiveBook { AccountId = account.AccountId, Cash = account.FundedCapital };
            db.Add(book);
        }
        if (book.Status != "Active" && !(liquidation && book.Status == "Closing"))
            return Result<PlaceOrderResult>.Fail("Account is awaiting capital reconciliation.");
        if (liquidation && (c.Side != "SELL" || book.CloseRequestId == null))
            return Result<PlaceOrderResult>.Fail("A liquidation request is required.");
        var position = await db.Set<LivePosition>()
            .SingleOrDefaultAsync(
                x => x.AccountId == account.AccountId && x.InstrumentToken == c.InstrumentToken,
                ct
            );
        if (c.Side == "BUY")
        {
            if (book.Cash - book.ReservedCash < value)
                return Result<PlaceOrderResult>.Fail("Insufficient buying power.");
            book.ReservedCash += value;
            order.ReservedCash = value;
            if (position == null)
            {
                position = new LivePosition
                {
                    AccountId = account.AccountId,
                    InstrumentToken = c.InstrumentToken,
                    Symbol = c.Symbol,
                };
                db.Add(position);
            }
        }
        else
        {
            if (position == null || position.Quantity - position.ReservedQuantity < c.Quantity)
                return Result<PlaceOrderResult>.Fail("Insufficient equity inventory.");
            position.ReservedQuantity += c.Quantity;
            db.Entry(book).Property(x => x.Cash).IsModified = true;
        }
        order.UsesLiveBook = true;
        order.Status = "PendingSubmission";
        db.Add(order);
        // The order intent and its capital/inventory reservation commit together before the broker call.
        await db.SaveChangesAsync(ct);
        BrokerOrderResponse response;
        try
        {
            response = await broker.PlaceAsync(
                new(
                    c.InstrumentToken,
                    c.Side,
                    c.Quantity,
                    c.OrderType,
                    c.Price,
                    order.OrderId.ToString("N"),
                    order.BrokerEnvironment,
                    order.BrokerCredentialKey,
                    order.BrokerProvider
                ),
                ct
            );
        }
        catch (Exception ex) when (ex is HttpRequestException or OperationCanceledException)
        {
            // A timeout does not prove rejection. Keep the reservation; never resubmit blindly.
            order.Status = "ReconciliationRequired";
            await db.SaveChangesAsync(CancellationToken.None);
            return Result<PlaceOrderResult>.Ok(new(order.OrderId, order.Status, null));
        }
        if (!response.Success)
        {
            // Releasing capital and persisting order state span module boundaries. Defer to reconciliation
            // instead of risking a double release on retries or a crash between writes.
            order.Status = "ReconciliationRequired";
        }
        else
        {
            order.Status = "Submitted";
            order.BrokerOrderId = response.BrokerOrderId;
            order.PlacedAt = DateTimeOffset.UtcNow;
        }
        db.Add(
            new OrderEvent
            {
                OrderEventId = Guid.NewGuid(),
                OrderInternalId = order.Id,
                EventType = order.Status,
                OldStatus = "PendingSubmission",
                NewStatus = order.Status,
                ProviderPayload = response.RawPayload,
            }
        );
        await db.SaveChangesAsync(CancellationToken.None);
        return Result<PlaceOrderResult>.Ok(new(order.OrderId, order.Status, order.BrokerOrderId));
    }
}
