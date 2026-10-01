using Microsoft.EntityFrameworkCore;
using MyFundex.BuildingBlocks.Abstractions;
using MyFundex.Contracts;
using MyFundex.FundedAccounts;
using MyFundex.MarketData;
using MyFundex.Subscription;
using MyFundex.Trading;
using MyFundex.Wallet;
using MyFundex.Withdrawals;

namespace MyFundex.Api.Infrastructure;

public static class LifecycleEndpoints
{
    public static void MapLifecycleEndpoints(this WebApplication app)
    {
        var api = app.MapGroup("/api/v1").RequireAuthorization();
        api.MapGet(
            "/accounts/{accountId:guid}/earnings",
            async (
                Guid accountId,
                IFundedAccountReader accounts,
                ICurrentActor actor,
                TradingDbContext db,
                CancellationToken ct
            ) =>
            {
                var account = await accounts.GetByPublicIdAsync(accountId, ct);
                if (
                    account == null
                    || account.UserId != actor.ActorId
                    || account.TradingMode != "Funded"
                )
                    return Results.NotFound();
                var executions = await (
                    from fill in db.Executions.AsNoTracking()
                    join order in db.Orders.AsNoTracking() on fill.OrderInternalId equals order.Id
                    where order.AccountId == accountId && order.Side == "SELL" && order.UsesLiveBook
                    orderby fill.ExecutedAt descending
                    select new
                    {
                        fill.ExecutionId,
                        order.OrderId,
                        order.Symbol,
                        fill.Quantity,
                        fill.Price,
                        fill.ExecutedAt,
                        fill.RealizedProfit,
                        fill.TraderSharePercent,
                        fill.TaxWithholdingPercent,
                        fill.OtherDeductionPercent,
                    }
                ).Take(200).ToListAsync(ct);
                var trades = executions.Select(x => new
                {
                    trade = x,
                    estimate = x.RealizedProfit.HasValue
                    && x.TraderSharePercent.HasValue
                    && x.TaxWithholdingPercent.HasValue
                    && x.OtherDeductionPercent.HasValue
                        ? ProfitAllocation.Calculate(
                            x.RealizedProfit.Value,
                            x.TraderSharePercent.Value,
                            x.TaxWithholdingPercent.Value,
                            x.OtherDeductionPercent.Value
                        )
                        : null,
                });
                var settlements = await db.Set<ProfitDistribution>()
                    .AsNoTracking()
                    .Where(x => x.AccountId == accountId)
                    .OrderByDescending(x => x.Id)
                    .Take(100)
                    .Select(x => new
                    {
                        x.DistributionId,
                        x.GrossProfit,
                        x.Fees,
                        x.TraderReward,
                        x.PlatformReward,
                        x.TaxWithheld,
                        x.OtherDeductions,
                        x.DeliveredAt,
                        x.CreatedAt,
                    })
                    .ToListAsync(ct);
                return Results.Ok(
                    new
                    {
                        trades,
                        settlements,
                        note = "Trade estimates exclude broker charges and account losses. Only verified net settlements become withdrawable. Withholding is not evidence of tax remittance.",
                    }
                );
            }
        );
        api.MapGet(
            "/challenges",
            async (
                SubscriptionDbContext db,
                TradingDbContext trading,
                ICurrentActor actor,
                CancellationToken ct
            ) =>
            {
                var attempts = await (
                    from attempt in db.Set<ChallengeAttempt>().AsNoTracking()
                    join subscription in db.Subscriptions.AsNoTracking()
                        on attempt.SubscriptionInternalId equals subscription.Id
                    join stage in db.Stages.AsNoTracking()
                        on attempt.StageInternalId equals stage.Id
                    join plan in db.Plans.AsNoTracking()
                        on subscription.PlanInternalId equals plan.Id
                    where subscription.UserInternalId == actor.ActorId
                    orderby attempt.Id descending
                    select new
                    {
                        attempt.AttemptId,
                        attempt.AccountId,
                        attempt.Status,
                        attempt.StartedAt,
                        attempt.CompletedAt,
                        subscription.SubscriptionId,
                        PlanName = plan.Name,
                        stage.StageNumber,
                        stage.StartingCapital,
                        stage.ProfitTargetPercent,
                        stage.MaxDailyLossPercent,
                        stage.MaxTotalLossPercent,
                        stage.MinimumTradingDays,
                        stage.MaximumCalendarDays,
                        stage.TradingPeriod,
                        stage.MaximumLeverage,
                    }
                ).Take(200).ToListAsync(ct);
                var ids = attempts
                    .Where(x => x.AccountId != null)
                    .Select(x => x.AccountId!.Value)
                    .ToArray();
                var books = await trading
                    .Set<PaperBook>()
                    .AsNoTracking()
                    .Where(x => ids.Contains(x.AccountId))
                    .ToDictionaryAsync(x => x.AccountId, ct);
                return Results.Ok(
                    attempts.Select(x => new
                    {
                        attempt = x,
                        progress = x.AccountId is Guid id && books.TryGetValue(id, out var book)
                            ? new
                            {
                                book.Cash,
                                book.Equity,
                                book.RealizedProfit,
                                book.TradingDays,
                                book.DayOpeningEquity,
                                book.ValuedAt,
                                book.Status,
                            }
                            : null,
                    })
                );
            }
        );
        api.MapGet(
            "/wallets",
            async (
                AccountsDbContext accounts,
                WalletDbContext wallets,
                ICurrentActor actor,
                CancellationToken ct
            ) =>
            {
                var ids = await accounts
                    .Accounts.AsNoTracking()
                    .Where(x => x.UserInternalId == actor.ActorId && x.TradingMode == "Funded")
                    .Select(x => new { x.Id, x.AccountId })
                    .ToListAsync(ct);
                var internalIds = ids.Select(x => x.Id).ToArray();
                var data = await wallets
                    .Wallets.AsNoTracking()
                    .Where(x => internalIds.Contains(x.FundedAccountInternalId))
                    .ToListAsync(ct);
                var lookup = ids.ToDictionary(x => x.Id, x => x.AccountId);
                return Results.Ok(
                    data.Select(x => new
                    {
                        accountId = lookup[x.FundedAccountInternalId],
                        x.WalletId,
                        x.CachedAvailableBalance,
                        x.CachedWithdrawableBalance,
                        x.CurrencyCode,
                        x.Status,
                    })
                );
            }
        );
        api.MapGet(
            "/wallets/{accountId:guid}/ledger",
            async (
                Guid accountId,
                IFundedAccountReader accounts,
                WalletDbContext db,
                ICurrentActor actor,
                CancellationToken ct
            ) =>
            {
                var account = await accounts.GetByPublicIdAsync(accountId, ct);
                if (account == null || account.UserId != actor.ActorId)
                    return Results.NotFound();
                return Results.Ok(await (
                        from entry in db.Entries.AsNoTracking()
                        join wallet in db.Wallets.AsNoTracking()
                            on entry.WalletInternalId equals wallet.Id
                        join transaction in db.Transactions.AsNoTracking()
                            on entry.TransactionInternalId equals transaction.Id
                        where
                            wallet.FundedAccountInternalId == account.InternalId
                            && entry.LedgerAccount == "TraderPayable"
                        orderby entry.Id descending
                        select new
                        {
                            entry.EntryId,
                            entry.Direction,
                            entry.Amount,
                            transaction.TransactionType,
                            transaction.ReferenceId,
                            transaction.OccurredAt,
                        }
                    ).Take(200).ToListAsync(ct));
            }
        );
        api.MapPost(
            "/withdrawals",
            async (WithdrawalInput input, WithdrawalService service, CancellationToken ct) =>
            {
                var item = await service.RequestAsync(
                    input.AccountId,
                    input.RequestId,
                    input.Amount,
                    ct
                );
                return Results.Ok(
                    new
                    {
                        item.WithdrawalId,
                        item.Status,
                        item.RequestedAmount,
                    }
                );
            }
        );
        api.MapGet(
            "/withdrawals",
            async (WithdrawalDbContext db, ICurrentActor actor, CancellationToken ct) =>
                Results.Ok(
                    await db
                        .Requests.AsNoTracking()
                        .Where(x => x.UserInternalId == actor.ActorId)
                        .OrderByDescending(x => x.Id)
                        .Take(200)
                        .Select(x => new
                        {
                            x.WithdrawalId,
                            x.RequestedAmount,
                            x.Status,
                            x.RequestedAt,
                        })
                        .ToListAsync(ct)
                )
        );

        var admin = app.MapGroup("/api/v1/admin/lifecycle")
            .RequireAuthorization(p => p.RequireRole("ADMIN"));
        admin.MapPost(
            "/accounts/{id:guid}/suspension",
            async (
                Guid id,
                SuspensionInput input,
                AccountAdministration service,
                CancellationToken ct
            ) =>
            {
                await service.SetSuspendedAsync(id, input.Suspend, input.Version, input.Reason, ct);
                return Results.NoContent();
            }
        );
        admin.MapGet(
            "/accounts/{id:guid}/history",
            async (Guid id, AccountsDbContext db, CancellationToken ct) => Results.Ok(await (
                        from history in db.StatusHistory.AsNoTracking()
                        join account in db.Accounts.AsNoTracking()
                            on history.FundedAccountInternalId equals account.Id
                        where account.AccountId == id
                        orderby history.Id descending
                        select new
                        {
                            history.FromStatus,
                            history.ToStatus,
                            history.Reason,
                            history.ReasonCode,
                            history.ChangedAt,
                            history.CreatedBy,
                        }
                    ).Take(200).ToListAsync(ct))
        );
        admin.MapPut(
            "/calendar",
            async (
                CalendarInput input,
                MarketDataDbContext db,
                IAuditWriter audit,
                CancellationToken ct
            ) =>
            {
                if (
                    input.Exchange is not ("NSE" or "BSE")
                    || input.IsTradingDay
                        && (input.Open == null || input.Close == null || input.Open >= input.Close)
                )
                    return Results.BadRequest(
                        new { message = "Provide a valid exchange and session times in IST." }
                    );
                var day = await db.TradingCalendar.SingleOrDefaultAsync(
                    x => x.ExchangeCode == input.Exchange && x.TradeDate == input.Date,
                    ct
                );
                if (day == null)
                {
                    day = new TradingCalendarDay
                    {
                        CalendarDayId = Guid.NewGuid(),
                        ExchangeCode = input.Exchange,
                        TradeDate = input.Date,
                    };
                    db.Add(day);
                }
                day.IsTradingDay = input.IsTradingDay;
                day.SessionOpen = input.Open;
                day.SessionClose = input.Close;
                await db.SaveChangesAsync(ct);
                await audit.WriteAsync(
                    "MarketData",
                    "Calendar",
                    day.CalendarDayId.ToString(),
                    "SessionUpdated",
                    null,
                    input.Date.ToString(),
                    ct
                );
                return Results.NoContent();
            }
        );
        admin.MapGet(
            "/subscriptions",
            async (SubscriptionDbContext db, CancellationToken ct) =>
                Results.Ok(
                    await db
                        .Subscriptions.AsNoTracking()
                        .OrderByDescending(x => x.Id)
                        .Take(500)
                        .Select(x => new
                        {
                            x.SubscriptionId,
                            x.UserInternalId,
                            x.Status,
                            x.SubscribedAt,
                            x.CompletedAt,
                        })
                        .ToListAsync(ct)
                )
        );
        admin.MapPost(
            "/activate-live",
            async (
                LiveInput input,
                ILiveAccountProvisioner service,
                IAuditWriter audit,
                IRuntimeSettings settings,
                CancellationToken ct
            ) =>
            {
                if (!bool.TryParse(await settings.GetAsync("Trading.LiveEnabled", "GLOBAL", ct), out var enabled) || !enabled)
                    return Results.Problem(
                        "Real trading is disabled in admin runtime settings.",
                        statusCode: 503
                    );
                var id = await service.ProvisionAsync(
                    input.SubscriptionId,
                    input.Provider,
                    input.CredentialKey,
                    input.BrokerUserId,
                    ct
                );
                await audit.WriteAsync(
                    "FundedAccounts",
                    "Account",
                    id.ToString(),
                    "LiveAllocationVerified",
                    null,
                    input.SubscriptionId.ToString(),
                    ct
                );
                return Results.Ok(new { accountId = id });
            }
        );
        admin.MapGet(
            "/withdrawals",
            async (WithdrawalDbContext db, CancellationToken ct) =>
                Results.Ok(
                    await db
                        .Requests.AsNoTracking()
                        .OrderByDescending(x => x.Id)
                        .Take(500)
                        .Select(x => new
                        {
                            x.WithdrawalId,
                            x.UserInternalId,
                            x.RequestedAmount,
                            x.Status,
                            x.RequestedAt,
                            x.ProviderPayoutId,
                        })
                        .ToListAsync(ct)
                )
        );
        admin.MapPost(
            "/withdrawals/{id:guid}/reconcile",
            async (
                Guid id,
                PayoutReconciliationInput input,
                WithdrawalService service,
                IAuditWriter audit,
                CancellationToken ct
            ) =>
            {
                await service.ReconcileProviderIdAsync(id, input.ProviderPayoutId, ct);
                await audit.WriteAsync(
                    "Withdrawals",
                    "Withdrawal",
                    id.ToString(),
                    "ProviderPayoutLinked",
                    null,
                    input.ProviderPayoutId,
                    ct
                );
                return Results.NoContent();
            }
        );
        admin.MapPost(
            "/withdrawals/{id:guid}/review",
            async (
                Guid id,
                ReviewInput input,
                WithdrawalService service,
                IAuditWriter audit,
                CancellationToken ct
            ) =>
            {
                await service.ReviewAsync(id, input.Approve, input.BeneficiaryReference, ct);
                await audit.WriteAsync(
                    "Withdrawals",
                    "Withdrawal",
                    id.ToString(),
                    input.Approve ? "Approved" : "Rejected",
                    null,
                    null,
                    ct
                );
                return Results.NoContent();
            }
        );
        admin.MapPost(
            "/accounts/{id:guid}/square-off",
            async (
                Guid id,
                SquareOffInput input,
                SquareOffService service,
                IAuditWriter audit,
                CancellationToken ct
            ) =>
            {
                await service.RequestAsync(id, input.Reason, ct);
                await audit.WriteAsync(
                    "Trading",
                    "Account",
                    id.ToString(),
                    "SquareOffRequested",
                    null,
                    input.Reason,
                    ct
                );
                return Results.Accepted(
                    value: new
                    {
                        status = "Closing",
                        message = "New exposure is blocked. Cancellation and closing orders will be reconciled with the broker.",
                    }
                );
            }
        );
        admin.MapPost(
            "/settlements",
            async (
                SettlementInput input,
                ProfitDistributionService service,
                IAuditWriter audit,
                CancellationToken ct
            ) =>
            {
                var item = await service.SettleAsync(
                    input.AccountId,
                    input.SubscriptionId,
                    input.RequestId,
                    input.VerifiedFees,
                    input.BrokerSettlementReference,
                    ct
                );
                await audit.WriteAsync(
                    "Trading",
                    "Settlement",
                    item.DistributionId.ToString(),
                    "BrokerSettlementConfirmed",
                    null,
                    input.BrokerSettlementReference,
                    ct
                );
                return Results.Ok(
                    new
                    {
                        item.DistributionId,
                        item.GrossProfit,
                        item.Fees,
                        item.TraderReward,
                        item.PlatformReward,
                    }
                );
            }
        );
        admin.MapPost(
            "/orders/{id:guid}/reconcile",
            async (Guid id, LiveSettlementService service, CancellationToken ct) =>
            {
                await service.ReconcileAsync(id, ct);
                return Results.NoContent();
            }
        );
        admin.MapGet(
            "/orders",
            async (TradingDbContext db, CancellationToken ct) =>
                Results.Ok(
                    await db
                        .Orders.AsNoTracking()
                        .OrderByDescending(x => x.Id)
                        .Take(500)
                        .Select(x => new
                        {
                            x.OrderId,
                            x.AccountId,
                            x.Symbol,
                            x.Side,
                            x.Quantity,
                            x.FilledQuantity,
                            x.Status,
                            x.BrokerEnvironment,
                            x.CreatedAt,
                        })
                        .ToListAsync(ct)
                )
        );
    }
}

public sealed record WithdrawalInput(Guid AccountId, Guid RequestId, decimal Amount);

public sealed record LiveInput(
    Guid SubscriptionId,
    string Provider,
    string CredentialKey,
    string BrokerUserId
);

public sealed record ReviewInput(bool Approve, string? BeneficiaryReference);

public sealed record SettlementInput(
    Guid AccountId,
    Guid SubscriptionId,
    Guid RequestId,
    decimal VerifiedFees,
    string BrokerSettlementReference
);

public sealed record CalendarInput(
    string Exchange,
    DateOnly Date,
    bool IsTradingDay,
    TimeOnly? Open,
    TimeOnly? Close
);

public sealed record SquareOffInput(string Reason);

public sealed record SuspensionInput(bool Suspend, long Version, string Reason);

public sealed record PayoutReconciliationInput(string ProviderPayoutId);
