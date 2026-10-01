using Microsoft.EntityFrameworkCore;
using MyFundex.BuildingBlocks.Abstractions;
using MyFundex.BuildingBlocks.Results;
using MyFundex.Contracts;

namespace MyFundex.Trading;

public sealed class PaperTradingEngine(
    TradingDbContext db,
    IFundedAccountReader accounts,
    ITradingEligibility eligibility,
    IEvaluationTermsReader termsReader,
    IMarketQuoteProvider quotes,
    IRiskGuard risk,
    ICurrentActor actor,
    IInstrumentCatalogue instruments
) : IEvaluationProgressReader
{
    public async Task<Result<PlaceOrderResult>> PlaceAsync(
        PlaceOrderCommand command,
        CancellationToken ct
    )
    {
        var error = OrderValidation.Validate(command);
        if (error != null)
            return Result<PlaceOrderResult>.Fail(error);
        var account = await accounts.GetByPublicIdAsync(command.AccountId, ct);
        if (
            account == null
            || actor.ActorId <= 0
            || account.UserId != actor.ActorId
            || account.TradingMode != "Evaluation"
        )
            return Result<PlaceOrderResult>.Fail("Paper account not found.");
        var previous = await db
            .Orders.AsNoTracking()
            .SingleOrDefaultAsync(x => x.IdempotencyKey == command.IdempotencyKey, ct);
        if (previous != null)
        {
            if (
                previous.AccountId != command.AccountId
                || previous.InstrumentToken != command.InstrumentToken
                || previous.Side != command.Side
                || previous.Quantity != command.Quantity
                || previous.OrderType != command.OrderType
                || previous.RequestedPrice != command.Price
            )
                return Result<PlaceOrderResult>.Fail("Order reference has already been used.");
            return Result<PlaceOrderResult>.Ok(
                new(previous.OrderId, previous.Status, previous.BrokerOrderId)
            );
        }
        var access = await eligibility.CheckAsync(
            account.AccountId,
            actor.ActorId,
            account.TradingMode,
            ct
        );
        if (account.Status != "Active" || !access.Allowed)
            return Result<PlaceOrderResult>.Fail(access.Reason ?? "Account is inactive.");
        var instrument = await instruments.GetAsync(command.InstrumentToken, ct);
        if (
            instrument == null
            || instrument.SecurityType != "EQUITY"
            || !await instruments.IsSessionOpenAsync(instrument.Exchange, ct)
        )
            return Result<PlaceOrderResult>.Fail(
                "Select an active equity during its configured trading session."
            );
        if (
            instrument.LotSize <= 0
            || command.Quantity % instrument.LotSize != 0
            || (
                command.OrderType == "LIMIT"
                && (instrument.TickSize <= 0 || command.Price!.Value % instrument.TickSize != 0)
            )
        )
            return Result<PlaceOrderResult>.Fail(
                "Order price or quantity does not match the instrument tick and lot size."
            );
        command = command with { Symbol = instrument.Symbol };
        var terms = await termsReader.GetAsync(account.AccountId, ct);
        if (terms == null)
            return Result<PlaceOrderResult>.Fail("Evaluation terms are unavailable.");
        var book = await LoadAsync(account, ct);
        var positions = await db.Set<PaperPosition>()
            .Where(x => x.AccountId == account.AccountId)
            .ToListAsync(ct);
        // Quote requests finish before the atomic SaveChanges transaction. The book version serializes account changes.
        await ValueAsync(book, positions, terms, ct);
        if (book.Status != "Active")
        {
            await db.SaveChangesAsync(ct);
            return Result<PlaceOrderResult>.Fail(
                "Evaluation has ended. Check your challenge status."
            );
        }
        var price = await quotes.GetLtpAsync(command.InstrumentToken, ct);
        if (price is null or <= 0)
            return Result<PlaceOrderResult>.Fail("Current market price is unavailable.");
        var policyPrice = command.Price ?? price.Value;
        var check = await risk.EvaluateAsync(
            new(
                account.InternalId,
                account.AccountId,
                command.InstrumentToken,
                command.Side,
                command.Quantity,
                policyPrice,
                command.Quantity * policyPrice
            ),
            ct
        );
        if (!check.Allowed)
            return Result<PlaceOrderResult>.Fail(
                check.Message ?? "Risk policy rejected the order."
            );
        var position = positions.SingleOrDefault(x => x.InstrumentToken == command.InstrumentToken);
        var value = command.Quantity * (command.Price ?? price.Value);
        if (command.Side == "BUY" && value > book.Cash - book.ReservedCash)
            return Result<PlaceOrderResult>.Fail("Insufficient available buying power.");
        if (
            command.Side == "SELL"
            && (
                position == null || command.Quantity > position.Quantity - position.ReservedQuantity
            )
        )
            return Result<PlaceOrderResult>.Fail("Insufficient unreserved equity inventory.");
        if (
            command.OrderType == "LIMIT"
            && (command.Side == "BUY" ? price > command.Price : price < command.Price)
        )
        {
            if (position == null)
            {
                if (positions.Count >= 100)
                    return Result<PlaceOrderResult>.Fail(
                        "The account instrument limit is reached."
                    );
                position = new PaperPosition
                {
                    AccountId = account.AccountId,
                    InstrumentToken = command.InstrumentToken,
                    Symbol = command.Symbol,
                };
                db.Add(position);
            }
            var pending = new Order
            {
                OrderId = Guid.NewGuid(),
                AccountId = account.AccountId,
                FundedAccountInternalId = account.InternalId,
                InstrumentToken = command.InstrumentToken,
                Symbol = command.Symbol,
                Side = command.Side,
                OrderType = "LIMIT",
                Quantity = command.Quantity,
                RequestedPrice = command.Price,
                EstimatedPrice = command.Price!.Value,
                ReservedCash = command.Side == "BUY" ? value : 0,
                BrokerProvider = "Paper",
                BrokerEnvironment = "SANDBOX",
                IdempotencyKey = command.IdempotencyKey,
                CorrelationId = Guid.NewGuid(),
                Status = "Open",
                PlacedAt = DateTimeOffset.UtcNow,
            };
            if (command.Side == "BUY")
                book.ReservedCash += value;
            else
                position.ReservedQuantity += command.Quantity;
            db.Add(pending);
            await db.SaveChangesAsync(ct);
            return Result<PlaceOrderResult>.Ok(new(pending.OrderId, pending.Status, null));
        }
        var fill = PaperAccounting.Fill(
            book.Cash,
            position?.Quantity ?? 0,
            position?.AverageCost ?? 0,
            command.Side,
            command.Quantity,
            price.Value
        );
        if (position == null)
        {
            if (positions.Count >= 100)
                return Result<PlaceOrderResult>.Fail("The account instrument limit is reached.");
            position = new PaperPosition
            {
                AccountId = account.AccountId,
                InstrumentToken = command.InstrumentToken,
                Symbol = command.Symbol,
            };
            db.Add(position);
            positions.Add(position);
        }
        book.Cash = fill.Cash;
        book.RealizedProfit += fill.Profit;
        position.Quantity = fill.Quantity;
        position.AverageCost = fill.AverageCost;
        position.RealizedPnl += fill.Profit;
        position.LastPrice = price.Value;
        if (book.LastTradeDate != book.TradingDate)
        {
            book.TradingDays++;
            book.LastTradeDate = book.TradingDate;
        }
        Decide(book, positions, terms);
        var order = new Order
        {
            OrderId = Guid.NewGuid(),
            AccountId = account.AccountId,
            FundedAccountInternalId = account.InternalId,
            InstrumentToken = command.InstrumentToken,
            Symbol = command.Symbol,
            Side = command.Side,
            OrderType = command.OrderType,
            Quantity = command.Quantity,
            RequestedPrice = command.Price,
            EstimatedPrice = price.Value,
            FilledQuantity = command.Quantity,
            AverageFillPrice = price,
            BrokerProvider = "Paper",
            BrokerEnvironment = "SANDBOX",
            IdempotencyKey = command.IdempotencyKey,
            CorrelationId = Guid.NewGuid(),
            Status = "Filled",
            PlacedAt = DateTimeOffset.UtcNow,
            CompletedAt = DateTimeOffset.UtcNow,
        };
        db.Add(order);
        // Relationship fix-up inserts order and fill in one transaction, with the capital/position changes.
        db.Add(
            new Execution
            {
                ExecutionId = Guid.NewGuid(),
                Order = order,
                Quantity = command.Quantity,
                Price = price.Value,
                GrossValue = command.Quantity * price.Value,
                ExecutedAt = DateTimeOffset.UtcNow,
            }
        );
        await db.SaveChangesAsync(ct);
        return Result<PlaceOrderResult>.Ok(new(order.OrderId, order.Status, null));
    }

    public async Task<EvaluationSnapshot?> GetAsync(Guid accountId, CancellationToken ct)
    {
        var account = await accounts.GetByPublicIdAsync(accountId, ct);
        var terms = await termsReader.GetAsync(accountId, ct);
        if (account == null || terms == null || account.TradingMode != "Evaluation")
            return null;
        var book = await LoadAsync(account, ct);
        var positions = await db.Set<PaperPosition>()
            .Where(x => x.AccountId == accountId)
            .ToListAsync(ct);
        if (book.Status == "Active")
            await ValueAsync(book, positions, terms, ct);
        await db.SaveChangesAsync(ct);
        return new(
            accountId,
            book.Status,
            book.Cash - book.ReservedCash,
            book.Equity,
            book.RealizedProfit,
            book.DayOpeningEquity,
            book.TradingDays,
            positions.Any(x => x.Quantity > 0),
            book.ValuedAt
        );
    }

    public async Task CancelAsync(Guid orderId, CancellationToken ct)
    {
        var order = await db.Orders.SingleAsync(x => x.OrderId == orderId, ct);
        if (order.BrokerProvider != "Paper")
            throw new ArgumentException("A paper order is required.");
        if (order.Status == "Cancelled")
            return;
        if (order.Status != "Open")
            throw new ArgumentException("Only an open paper order can be cancelled.");
        var book = await db.Set<PaperBook>().SingleAsync(x => x.AccountId == order.AccountId, ct);
        var position = await db.Set<PaperPosition>()
            .SingleAsync(
                x => x.AccountId == order.AccountId && x.InstrumentToken == order.InstrumentToken,
                ct
            );
        CancelPending(order, book, position);
        db.Entry(book).Property(x => x.Cash).IsModified = true;
        await db.SaveChangesAsync(ct);
    }

    public async Task MatchAsync(Guid orderId, CancellationToken ct)
    {
        var order = await db.Orders.SingleAsync(x => x.OrderId == orderId, ct);
        if (order.Status != "Open" || order.BrokerProvider != "Paper")
            return;
        var account =
            await accounts.GetByPublicIdAsync(order.AccountId, ct)
            ?? throw new ArgumentException("Paper account missing.");
        var book = await LoadAsync(account, ct);
        var positions = await db.Set<PaperPosition>()
            .Where(x => x.AccountId == order.AccountId)
            .ToListAsync(ct);
        var position = positions.Single(x => x.InstrumentToken == order.InstrumentToken);
        var instrument = await instruments.GetAsync(order.InstrumentToken, ct);
        var access = await eligibility.CheckAsync(
            order.AccountId,
            account.UserId,
            "Evaluation",
            ct
        );
        if (
            account.Status != "Active"
            || book.Status != "Active"
            || !access.Allowed
            || instrument == null
            || IndiaDate(order.PlacedAt!.Value) != IndiaDate(DateTimeOffset.UtcNow)
            || !await instruments.IsSessionOpenAsync(instrument.Exchange, ct)
        )
        {
            CancelPending(order, book, position);
            db.Entry(book).Property(x => x.Cash).IsModified = true;
            await db.SaveChangesAsync(ct);
            return;
        }
        var terms =
            await termsReader.GetAsync(order.AccountId, ct)
            ?? throw new ArgumentException("Evaluation terms missing.");
        await ValueAsync(book, positions, terms, ct);
        if (book.Status != "Active")
        {
            await db.SaveChangesAsync(ct);
            return;
        }
        var price = await quotes.GetLtpAsync(order.InstrumentToken, ct);
        if (
            price is null or <= 0
            || (order.Side == "BUY" ? price > order.RequestedPrice : price < order.RequestedPrice)
        )
            return;
        // Release only this order's reservation and atomically apply its simulated full fill.
        if (order.Side == "BUY")
        {
            book.ReservedCash -= order.ReservedCash;
            order.ReservedCash = 0;
        }
        else
            position.ReservedQuantity -= order.Quantity;
        var fill = PaperAccounting.Fill(
            book.Cash,
            position.Quantity,
            position.AverageCost,
            order.Side,
            order.Quantity,
            price.Value
        );
        book.Cash = fill.Cash;
        book.RealizedProfit += fill.Profit;
        position.Quantity = fill.Quantity;
        position.AverageCost = fill.AverageCost;
        position.RealizedPnl += fill.Profit;
        position.LastPrice = price.Value;
        if (book.LastTradeDate != book.TradingDate)
        {
            book.TradingDays++;
            book.LastTradeDate = book.TradingDate;
        }
        order.Status = "Filled";
        order.FilledQuantity = order.Quantity;
        order.AverageFillPrice = price;
        order.CompletedAt = DateTimeOffset.UtcNow;
        db.Add(
            new Execution
            {
                ExecutionId = Guid.NewGuid(),
                OrderInternalId = order.Id,
                Quantity = order.Quantity,
                Price = price.Value,
                GrossValue = order.Quantity * price.Value,
                ExecutedAt = DateTimeOffset.UtcNow,
            }
        );
        Decide(book, positions, terms);
        await db.SaveChangesAsync(ct);
    }

    private void CancelPending(Order order, PaperBook book, PaperPosition position)
    {
        if (order.Side == "BUY")
        {
            book.ReservedCash -= order.ReservedCash;
            order.ReservedCash = 0;
        }
        else
            position.ReservedQuantity -= order.Quantity;
        order.Status = "Cancelled";
        order.CompletedAt = DateTimeOffset.UtcNow;
        db.OrderEvents.Add(
            new OrderEvent
            {
                OrderEventId = Guid.NewGuid(),
                OrderInternalId = order.Id,
                EventType = "PaperCancellation",
                OldStatus = "Open",
                NewStatus = "Cancelled",
            }
        );
    }

    private async Task<PaperBook> LoadAsync(FundedAccountSnapshot account, CancellationToken ct)
    {
        var book = await db.Set<PaperBook>()
            .SingleOrDefaultAsync(x => x.AccountId == account.AccountId, ct);
        if (book != null)
            return book;
        if (
            await db.Orders.AnyAsync(
                x => x.AccountId == account.AccountId && x.BrokerProvider != "Paper",
                ct
            )
        )
            throw new ArgumentException(
                "Existing broker orders must be reconciled before using the internal paper engine."
            );
        book = new PaperBook
        {
            AccountId = account.AccountId,
            AccountInternalId = account.InternalId,
            Cash = account.FundedCapital,
            Equity = account.FundedCapital,
            DayOpeningEquity = account.FundedCapital,
            TradingDate = IndiaDate(DateTimeOffset.UtcNow),
            ValuedAt = DateTimeOffset.UtcNow,
        };
        db.Add(book);
        return book;
    }

    private async Task ValueAsync(
        PaperBook book,
        List<PaperPosition> positions,
        EvaluationTerms terms,
        CancellationToken ct
    )
    {
        var today = IndiaDate(DateTimeOffset.UtcNow);
        // Previous valuation is the day's opening reference; overnight positions require a recorded closing valuation.
        if (book.TradingDate != today)
        {
            book.DayOpeningEquity = book.Equity;
            book.TradingDate = today;
        }
        var prices = await quotes.GetLtpsAsync(
            positions.Where(x => x.Quantity > 0).Select(x => x.InstrumentToken),
            ct
        );
        foreach (var position in positions.Where(x => x.Quantity > 0))
        {
            decimal? quote = prices.TryGetValue(position.InstrumentToken, out var current)
                ? current
                : null;
            if (quote is null or <= 0)
                throw new ArgumentException(
                    "Portfolio quote unavailable; evaluation is paused until valuation succeeds."
                );
            position.LastPrice = quote.Value;
        }
        book.ValuedAt = DateTimeOffset.UtcNow;
        Decide(book, positions, terms);
        if (book.Status == "Failed")
        {
            var pending = await db
                .Orders.Where(x =>
                    x.AccountId == book.AccountId
                    && x.BrokerProvider == "Paper"
                    && x.Status == "Open"
                )
                .ToListAsync(ct);
            foreach (var order in pending)
                CancelPending(
                    order,
                    book,
                    positions.Single(x => x.InstrumentToken == order.InstrumentToken)
                );
            CloseFailedPositions(book, positions);
        }
        db.Entry(book).Property(x => x.ValuedAt).IsModified =
            db.Entry(book).State != EntityState.Added;
    }

    private void CloseFailedPositions(PaperBook book, IEnumerable<PaperPosition> positions)
    {
        foreach (var position in positions.Where(x => x.Quantity > 0))
        {
            var quantity = position.Quantity;
            var fill = PaperAccounting.Fill(
                book.Cash,
                quantity,
                position.AverageCost,
                "SELL",
                quantity,
                position.LastPrice
            );
            book.Cash = fill.Cash;
            book.RealizedProfit += fill.Profit;
            position.RealizedPnl += fill.Profit;
            position.Quantity = 0;
            position.AverageCost = 0;
            var order = new Order
            {
                OrderId = Guid.NewGuid(),
                AccountId = book.AccountId,
                FundedAccountInternalId = book.AccountInternalId,
                InstrumentToken = position.InstrumentToken,
                Symbol = position.Symbol,
                Side = "SELL",
                Quantity = quantity,
                EstimatedPrice = position.LastPrice,
                AverageFillPrice = position.LastPrice,
                FilledQuantity = quantity,
                BrokerProvider = "Paper",
                BrokerEnvironment = "SANDBOX",
                IsLiquidation = true,
                IdempotencyKey = Guid.NewGuid(),
                CorrelationId = Guid.NewGuid(),
                Status = "Filled",
                PlacedAt = DateTimeOffset.UtcNow,
                CompletedAt = DateTimeOffset.UtcNow,
            };
            db.Add(order);
            db.Add(
                new Execution
                {
                    ExecutionId = Guid.NewGuid(),
                    Order = order,
                    Quantity = quantity,
                    Price = position.LastPrice,
                    GrossValue = quantity * position.LastPrice,
                    ExecutedAt = DateTimeOffset.UtcNow,
                }
            );
        }
        book.Equity = book.Cash;
    }

    private static void Decide(PaperBook book, List<PaperPosition> positions, EvaluationTerms terms)
    {
        book.Equity = book.Cash + positions.Sum(x => x.Quantity * x.LastPrice);
        if (
            terms.Capital - book.Equity >= terms.Capital * terms.TotalLossPercent / 100
            || book.DayOpeningEquity - book.Equity
                >= book.DayOpeningEquity * terms.DailyLossPercent / 100
            || terms.MaximumDays is int days
                && IndiaDate(DateTimeOffset.UtcNow).DayNumber - IndiaDate(terms.StartedAt).DayNumber
                    > days
        )
            book.Status = "Failed";
        else if (
            book.RealizedProfit >= terms.Capital * terms.ProfitTargetPercent / 100
            && book.TradingDays >= terms.MinimumDays
            && book.ReservedCash == 0
            && positions.All(x => x.Quantity == 0 && x.ReservedQuantity == 0)
        )
            book.Status = "Passed";
    }

    private static DateOnly IndiaDate(DateTimeOffset time) =>
        DateOnly.FromDateTime(time.ToOffset(TimeSpan.FromMinutes(330)).DateTime);
}
