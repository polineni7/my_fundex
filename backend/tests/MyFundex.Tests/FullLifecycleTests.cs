using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using MyFundex.Broker;
using MyFundex.BuildingBlocks.Abstractions;
using MyFundex.Contracts;
using MyFundex.FundedAccounts;
using MyFundex.Subscription;
using MyFundex.Trading;
using MyFundex.Wallet;
using MyFundex.Withdrawals;
using Xunit;

namespace MyFundex.Tests;

public sealed class FullLifecycleTests
{
    [Fact]
    public async Task PaidTwoStageEvaluationGraduatesOnlyAfterBothStagesAndReplayIsHarmless()
    {
        using var fixture = new Fixture();
        var subscription = await fixture.PurchaseAsync();
        Assert.Equal("PendingPayment", subscription.Status);
        await fixture.Activator.ActivateAsync(subscription.SubscriptionId, 1, default);
        for (var stage = 1; stage <= 2; stage++)
        {
            var attempt = await fixture
                .Subscriptions.Set<ChallengeAttempt>()
                .SingleAsync(x => x.Status == "InProgress");
            var accountId = attempt.AccountId!.Value;
            Assert.Null(await fixture.Entitlements.GetAsync(subscription.SubscriptionId, default));
            fixture.Quotes.Price = 100;
            var buy = Command(accountId, "BUY", 100);
            Assert.True((await fixture.Paper.PlaceAsync(buy, default)).Success);
            Assert.True((await fixture.Paper.PlaceAsync(buy, default)).Success);
            Assert.Equal(stage * 2 - 1, await fixture.Trading.Executions.CountAsync());
            fixture.Quotes.Price = 110;
            Assert.True(
                (await fixture.Paper.PlaceAsync(Command(accountId, "SELL", 100), default)).Success
            );
            var book = await fixture
                .Trading.Set<PaperBook>()
                .SingleAsync(x => x.AccountId == accountId);
            Assert.Equal("Passed", book.Status);
            Assert.Equal(11000, book.Cash);
            await fixture.Progression.AdvanceAsync(attempt.AttemptId, default);
            await fixture.Progression.AdvanceAsync(attempt.AttemptId, default);
            Assert.Equal(
                "Passed",
                (await fixture.Accounts.Accounts.SingleAsync(x => x.AccountId == accountId)).Status
            );
        }
        Assert.Equal("Completed", subscription.Status);
        Assert.Single(await fixture.Subscriptions.Set<ChallengeNotice>().ToListAsync());
        Assert.Equal(2, await fixture.Subscriptions.Set<ChallengeAttempt>().CountAsync());
        Assert.NotNull(await fixture.Entitlements.GetAsync(subscription.SubscriptionId, default));
        var live = new LiveAccountProvisioner(
            fixture.Accounts,
            fixture.Entitlements,
            new VerifyBroker(),
            fixture.WalletProvisioner,
            new Policies()
        );
        var id = await live.ProvisionAsync(
            subscription.SubscriptionId,
            "Upstox",
            "linked-account",
            "broker-user",
            default
        );
        Assert.Equal(
            id,
            await live.ProvisionAsync(
                subscription.SubscriptionId,
                "Upstox",
                "linked-account",
                "broker-user",
                default
            )
        );
        var eligibility = new TradingEligibilityService(
            fixture.Subscriptions,
            fixture.AccountReader,
            fixture.Entitlements
        );
        Assert.True((await eligibility.CheckAsync(id, 1, "Funded", default)).Allowed);
        Assert.False((await eligibility.CheckAsync(id, 2, "Funded", default)).Allowed);
    }

    [Fact]
    public async Task FailedAttemptIsLiquidatedAndRepurchaseCreatesFreshPaperCapital()
    {
        using var fixture = new Fixture();
        var subscription = await fixture.PurchaseAsync();
        await fixture.Activator.ActivateAsync(subscription.SubscriptionId, 1, default);
        var attempt = await fixture.Subscriptions.Set<ChallengeAttempt>().SingleAsync();
        var id = attempt.AccountId!.Value;
        fixture.Quotes.Price = 100;
        Assert.True((await fixture.Paper.PlaceAsync(Command(id, "BUY", 100), default)).Success);
        fixture.Quotes.Price = 90;
        await fixture.Progression.AdvanceAsync(attempt.AttemptId, default);
        Assert.Equal("Failed", subscription.Status);
        Assert.Equal("Failed", (await fixture.Accounts.Accounts.SingleAsync()).Status);
        Assert.Equal(9000, (await fixture.Trading.Set<PaperBook>().SingleAsync()).Cash);
        Assert.Equal(0, (await fixture.Trading.Set<PaperPosition>().SingleAsync()).Quantity);
        Assert.Equal(2, await fixture.Trading.Executions.CountAsync());
        Assert.False((await fixture.Paper.PlaceAsync(Command(id, "BUY", 1), default)).Success);
        var next = await fixture.Purchases.CreateAsync(
            1,
            fixture.VersionId,
            Guid.NewGuid(),
            subscription.SubscriptionId,
            default
        );
        Assert.Equal("PendingPayment", next.Status);
        await fixture.Activator.ActivateAsync(next.SubscriptionId, 1, default);
        var nextAttempt = await fixture
            .Subscriptions.Set<ChallengeAttempt>()
            .SingleAsync(x => x.SubscriptionInternalId == next.Id);
        var account = await fixture.Accounts.Accounts.SingleAsync(x =>
            x.AccountId == nextAttempt.AccountId
        );
        Assert.NotEqual(id, account.AccountId);
        Assert.Equal(10000, account.CurrentBuyingPower);
        Assert.Equal("Failed", subscription.Status);
        Assert.Null(await fixture.Entitlements.GetAsync(subscription.SubscriptionId, default));
    }

    [Fact]
    public async Task PaperCannotOversellOrBuyWithMissingQuoteAndOtherUserCannotTrade()
    {
        using var fixture = new Fixture();
        var subscription = await fixture.PurchaseAsync();
        await fixture.Activator.ActivateAsync(subscription.SubscriptionId, 1, default);
        var id = (await fixture.Accounts.Accounts.SingleAsync()).AccountId;
        Assert.False((await fixture.Paper.PlaceAsync(Command(id, "SELL", 1), default)).Success);
        fixture.Trading.ChangeTracker.Clear();
        fixture.Quotes.Price = null;
        Assert.False((await fixture.Paper.PlaceAsync(Command(id, "BUY", 1), default)).Success);
        Assert.Empty(await fixture.Trading.Orders.ToListAsync());
    }

    [Fact]
    public async Task RestingPaperOrdersReserveCapitalCancelAndFillWithoutDuplicatingExecutions()
    {
        using var fixture = new Fixture();
        var subscription = await fixture.PurchaseAsync();
        await fixture.Activator.ActivateAsync(subscription.SubscriptionId, 1, default);
        var id = (await fixture.Accounts.Accounts.SingleAsync()).AccountId;
        fixture.Quotes.Price = 100;
        var command = Command(id, "BUY", 100) with { OrderType = "LIMIT", Price = 90 };
        var placed = await fixture.Paper.PlaceAsync(command, default);
        Assert.True(placed.Success);
        var order = await fixture.Trading.Orders.SingleAsync();
        Assert.Equal("Open", order.Status);
        Assert.Equal(9000, (await fixture.Trading.Set<PaperBook>().SingleAsync()).ReservedCash);
        Assert.False((await fixture.Paper.PlaceAsync(Command(id, "BUY", 11), default)).Success);
        await fixture.Paper.CancelAsync(order.OrderId, default);
        await fixture.Paper.CancelAsync(order.OrderId, default);
        Assert.Equal(0, (await fixture.Trading.Set<PaperBook>().SingleAsync()).ReservedCash);
        var second = command with { IdempotencyKey = Guid.NewGuid() };
        await fixture.Paper.PlaceAsync(second, default);
        var pending = await fixture.Trading.Orders.SingleAsync(x => x.Status == "Open");
        fixture.Quotes.Price = 89;
        await fixture.Paper.MatchAsync(pending.OrderId, default);
        await fixture.Paper.MatchAsync(pending.OrderId, default);
        Assert.Equal("Filled", pending.Status);
        var book = await fixture.Trading.Set<PaperBook>().SingleAsync();
        Assert.Equal(1100, book.Cash);
        Assert.Equal(0, book.ReservedCash);
        Assert.Single(await fixture.Trading.Executions.ToListAsync());
    }

    [Fact]
    public async Task LivePartialFillCancellationSettlesExactlyOnceAndReleasesOnlyRemainder()
    {
        using var db = Trading();
        var accountId = Guid.NewGuid();
        var book = new LiveBook
        {
            AccountId = accountId,
            Cash = 10000,
            ReservedCash = 1000,
        };
        var position = new LivePosition { AccountId = accountId, InstrumentToken = "TEST" };
        var order = new Order
        {
            OrderId = Guid.NewGuid(),
            AccountId = accountId,
            InstrumentToken = "TEST",
            Side = "BUY",
            Quantity = 10,
            EstimatedPrice = 100,
            ReservedCash = 1000,
            UsesLiveBook = true,
            BrokerEnvironment = "PRODUCTION",
            Status = "Submitted",
        };
        db.AddRange(book, position, order);
        await db.SaveChangesAsync();
        var reader = new Reader
        {
            Observation = new(
                "broker",
                "open",
                "TEST",
                "BUY",
                10,
                4,
                [new("fill-1", 4, 95, DateTimeOffset.UtcNow)]
            ),
        };
        var service = new LiveSettlementService(db, reader);
        await service.ReconcileAsync(order.OrderId, default);
        await service.ReconcileAsync(order.OrderId, default);
        Assert.Equal(9620, book.Cash);
        Assert.Equal(600, book.ReservedCash);
        Assert.Equal(4, position.Quantity);
        Assert.Single(await db.Executions.ToListAsync());
        reader.Observation = reader.Observation with { Status = "cancelled" };
        await service.ReconcileAsync(order.OrderId, default);
        await service.ReconcileAsync(order.OrderId, default);
        Assert.Equal("Cancelled", order.Status);
        Assert.Equal(0, book.ReservedCash);
        Assert.Equal(9620, book.Cash);
    }

    [Fact]
    public async Task InconsistentBrokerFillsCannotChangeBalances()
    {
        using var db = Trading();
        var order = new Order
        {
            OrderId = Guid.NewGuid(),
            UsesLiveBook = true,
            BrokerEnvironment = "PRODUCTION",
            InstrumentToken = "TEST",
            Quantity = 2,
            Side = "BUY",
        };
        db.Add(order);
        await db.SaveChangesAsync();
        var reader = new Reader
        {
            Observation = new(
                "broker",
                "complete",
                "TEST",
                "BUY",
                2,
                2,
                [new("one", 1, 10, DateTimeOffset.UtcNow)]
            ),
        };
        await Assert.ThrowsAsync<ArgumentException>(
            () => new LiveSettlementService(db, reader).ReconcileAsync(order.OrderId, default)
        );
        Assert.Empty(await db.Executions.ToListAsync());
    }

    [Fact]
    public async Task RewardDistributionWithdrawalAndReversalKeepBalancedLedgerAndAreIdempotent()
    {
        using var fixture = new Fixture();
        var accountId = Guid.NewGuid();
        var account = new FundedAccount
        {
            AccountId = accountId,
            UserInternalId = 1,
            TradingMode = "Funded",
            Status = "Active",
            SubscriptionInternalId = 10,
            FundedCapital = 10000,
        };
        fixture.Accounts.Add(account);
        await fixture.Accounts.SaveChangesAsync();
        await fixture.WalletProvisioner.EnsureAsync(account.Id, default);
        fixture.Trading.Add(
            new LiveBook
            {
                AccountId = accountId,
                Cash = 11000,
                Equity = 11000,
                DayOpeningEquity = 10000,
            }
        );
        await fixture.Trading.SaveChangesAsync();
        var subscriptionId = Guid.NewGuid();
        var entitlement = new Entitlement(new(subscriptionId, 10, 1, 10000, Guid.NewGuid(), 80));
        var distribution = new ProfitDistributionService(
            fixture.Trading,
            fixture.AccountReader,
            entitlement,
            new WalletService(fixture.Wallets)
        );
        var reference = Guid.NewGuid();
        var posted = await distribution.SettleAsync(
            accountId,
            subscriptionId,
            reference,
            100,
            "broker-settlement-1",
            default
        );
        Assert.Equal(720, posted.TraderReward);
        Assert.Equal(180, posted.PlatformReward);
        await distribution.DeliverAsync(reference, default);
        await distribution.DeliverAsync(reference, default);
        Assert.Equal(10000, (await fixture.Trading.Set<LiveBook>().SingleAsync()).Cash);
        var funds = new WithdrawalFunds(fixture.Wallets);
        var payout = new Payout();
        using var withdrawalDb = new WithdrawalDbContext(
            Options<WithdrawalDbContext>(),
            new Actor()
        );
        var service = new WithdrawalService(
            withdrawalDb,
            fixture.AccountReader,
            funds,
            payout,
            new Actor()
        );
        var request = await service.RequestAsync(accountId, Guid.NewGuid(), 500, default);
        Assert.Equal(220, (await fixture.Wallets.Wallets.SingleAsync()).CachedWithdrawableBalance);
        await service.ReviewAsync(request.WithdrawalId, true, "fa_verified", default);
        await service.ProcessAsync(request.WithdrawalId, default);
        Assert.Equal("Paid", request.Status);
        await service.ProcessAsync(request.WithdrawalId, default);
        payout.Status = "reversed";
        await service.ProcessAsync(request.WithdrawalId, default);
        await service.ProcessAsync(request.WithdrawalId, default);
        Assert.Equal(720, (await fixture.Wallets.Wallets.SingleAsync()).CachedWithdrawableBalance);
        var entries = await fixture.Wallets.Entries.ToListAsync();
        Assert.Equal(6, entries.Count);
        Assert.All(
            entries.GroupBy(x => x.TransactionInternalId),
            group =>
                Assert.Equal(
                    group.Where(x => x.Direction == "Debit").Sum(x => x.Amount),
                    group.Where(x => x.Direction == "Credit").Sum(x => x.Amount)
                )
        );
    }

    [Fact]
    public void QuoteIdentityAndAgeAreValidated()
    {
        var now = DateTimeOffset.UtcNow;
        using var fresh = JsonDocument.Parse(
            JsonSerializer.Serialize(
                new
                {
                    data = new
                    {
                        stock = new
                        {
                            instrument_token = "TEST",
                            last_price = 100m,
                            last_trade_time = now.ToUnixTimeMilliseconds().ToString(),
                        },
                    },
                }
            )
        );
        Assert.Equal(100, UpstoxMarketQuoteProvider.ReadFreshPrice(fresh.RootElement, "TEST", now));
        Assert.Null(UpstoxMarketQuoteProvider.ReadFreshPrice(fresh.RootElement, "OTHER", now));
        Assert.Null(
            UpstoxMarketQuoteProvider.ReadFreshPrice(fresh.RootElement, "TEST", now.AddMinutes(3))
        );
        Assert.Null(
            UpstoxMarketQuoteProvider.ReadFreshPrice(fresh.RootElement, "TEST", now.AddMinutes(-1))
        );
    }

    [Fact]
    public async Task FundedLossBoundaryClosesExposureAndKeepsOneCloseRequest()
    {
        using var db = Trading();
        var id = Guid.NewGuid();
        var book = new LiveBook
        {
            AccountId = id,
            Cash = 9000,
            Equity = 10000,
            DayOpeningEquity = 10000,
        };
        db.AddRange(
            book,
            new LivePosition
            {
                AccountId = id,
                InstrumentToken = "TEST",
                Quantity = 10,
                AverageCost = 100,
            }
        );
        await db.SaveChangesAsync();
        var prices = new Quotes { Price = 50 };
        var monitor = new LiveRiskMonitor(db, prices, new LiveTerms());
        await monitor.CheckAsync(id, default);
        Assert.Equal(9500, book.Equity);
        Assert.Equal("Closing", book.Status);
        var request = book.CloseRequestId;
        Assert.NotNull(request);
        await monitor.CheckAsync(id, default);
        Assert.Equal(request, book.CloseRequestId);
    }

    [Fact]
    public async Task InterruptedCancellationRetriesOnlyAfterBrokerConfirmsOpenOrder()
    {
        using var db = Trading();
        var id = Guid.NewGuid();
        var order = new Order
        {
            OrderId = Guid.NewGuid(),
            AccountId = id,
            InstrumentToken = "TEST",
            Side = "BUY",
            Quantity = 10,
            EstimatedPrice = 100,
            ReservedCash = 1000,
            UsesLiveBook = true,
            BrokerEnvironment = "PRODUCTION",
            Status = "CancellationRequested",
        };
        var book = new LiveBook
        {
            AccountId = id,
            Cash = 10000,
            ReservedCash = 1000,
        };
        db.AddRange(order, book, new LivePosition { AccountId = id, InstrumentToken = "TEST" });
        await db.SaveChangesAsync();
        var reader = new Reader { Observation = new("broker", "open", "TEST", "BUY", 10, 0, []) };
        var cancel = new Cancellation();
        var service = new LiveSettlementService(db, reader, cancel);
        await service.ReconcileAsync(order.OrderId, default);
        Assert.Equal(1, cancel.Calls);
        Assert.Equal(1000, book.ReservedCash);
        reader.Observation = reader.Observation with { Status = "cancelled" };
        await service.ReconcileAsync(order.OrderId, default);
        Assert.Equal(1, cancel.Calls);
        Assert.Equal(0, book.ReservedCash);
        Assert.Equal("Cancelled", order.Status);
    }

    private sealed class LiveTerms : ILiveRiskTermsReader
    {
        public Task<LiveEntitlement?> GetByAccountAsync(Guid id, CancellationToken ct) =>
            Task.FromResult<LiveEntitlement?>(new(Guid.NewGuid(), 1, 1, 10000, Guid.NewGuid(), 80));
    }

    private sealed class Cancellation : IOrderCancellationGateway
    {
        public int Calls;

        public Task<BrokerCancellation> CancelAsync(
            BrokerOrderReference request,
            CancellationToken ct
        )
        {
            Calls++;
            return Task.FromResult(new BrokerCancellation(true, null));
        }
    }

    [Fact]
    public void ProfitAllocationSeparatesShareTaxAndOtherAndDoesNotPayLosses()
    {
        var split = ProfitAllocation.Calculate(1000, 80, 10, 5);
        Assert.Equal(800, split.TraderGross);
        Assert.Equal(200, split.Platform);
        Assert.Equal(80, split.Tax);
        Assert.Equal(40, split.Other);
        Assert.Equal(680, split.Net);
        Assert.Equal(1000, split.Net + split.Platform + split.Tax + split.Other);
        Assert.Equal(0, ProfitAllocation.Calculate(-100, 80, 10, 5).Net);
        Assert.Throws<ArgumentException>(() => ProfitAllocation.Calculate(100, 80, 80, 20));
    }

    [Fact]
    public async Task AllocationLedgerBalancesDeductionsAndRejectsChangedReplay()
    {
        using var fixture = new Fixture();
        await fixture.WalletProvisioner.EnsureAsync(42, default);
        var service = new WalletService(fixture.Wallets);
        await service.CreditProfitAllocationAsync(42, 680, 80, 40, "Settlement", "one", default);
        await service.CreditProfitAllocationAsync(42, 680, 80, 40, "Settlement", "one", default);
        var entries = await fixture.Wallets.Entries.ToListAsync();
        Assert.Equal(4, entries.Count);
        Assert.Equal(
            entries.Where(x => x.Direction == "Debit").Sum(x => x.Amount),
            entries.Where(x => x.Direction == "Credit").Sum(x => x.Amount)
        );
        Assert.Equal(680, (await service.GetByAccountAsync(42, default))!.WithdrawableBalance);
        await Assert.ThrowsAsync<ArgumentException>(
            () => service.CreditProfitAllocationAsync(42, 680, 81, 39, "Settlement", "one", default)
        );
    }

    private static PlaceOrderCommand Command(Guid accountId, string side, decimal quantity) =>
        new(accountId, "TEST", "TEST", side, "MARKET", quantity, null, Guid.NewGuid());

    private static DbContextOptions<T> Options<T>()
        where T : DbContext =>
        new DbContextOptionsBuilder<T>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;

    private static TradingDbContext Trading() => new(Options<TradingDbContext>(), new Actor());

    private sealed class Actor : ICurrentActor
    {
        public long ActorId => 1;
    }

    private sealed class Quotes : IMarketQuoteProvider
    {
        public decimal? Price = 100;

        public Task<decimal?> GetLtpAsync(string token, CancellationToken ct) =>
            Task.FromResult(Price);
    }

    private sealed class Risk : IRiskGuard
    {
        public Task<RiskCheckResult> EvaluateAsync(
            RiskCheckRequest request,
            CancellationToken ct
        ) => Task.FromResult(new RiskCheckResult(true));
    }

    private sealed class Instruments : IInstrumentCatalogue
    {
        public Task<TradableInstrument?> GetAsync(string token, CancellationToken ct) =>
            Task.FromResult<TradableInstrument?>(new(token, token, "NSE", "EQUITY", 0.05m, 1));

        public Task<bool> IsSessionOpenAsync(string exchange, CancellationToken ct) =>
            Task.FromResult(true);
    }

    private sealed class Policies : IPolicyAssignmentService
    {
        public Task AssignActiveAsync(long id, Guid policy, CancellationToken ct) =>
            Task.CompletedTask;
    }

    private sealed class VerifyBroker : IBrokerAccountVerifier
    {
        public Task<bool> VerifyAsync(
            string provider,
            string key,
            string user,
            CancellationToken ct
        ) => Task.FromResult(true);
    }

    private sealed class Reader : IBrokerOrderReader
    {
        public BrokerObservation Observation = null!;

        public Task<BrokerObservation?> ReadAsync(
            string provider,
            string credentialKey,
            string? orderId,
            string tag,
            CancellationToken ct
        ) => Task.FromResult<BrokerObservation?>(Observation);
    }

    private sealed class Entitlement(LiveEntitlement value) : ILiveEntitlementReader
    {
        public Task<LiveEntitlement?> GetAsync(Guid id, CancellationToken ct) =>
            Task.FromResult<LiveEntitlement?>(value);
    }

    private sealed class Payout : IPayoutGateway
    {
        public string Status = "processed";
        private Guid reference;

        public Task<PayoutResult> SendAsync(
            Guid id,
            string beneficiary,
            decimal amount,
            CancellationToken ct
        )
        {
            reference = id;
            return Task.FromResult(new PayoutResult("payout", Status, amount, id.ToString("N")));
        }

        public Task<PayoutResult> ReadAsync(string id, CancellationToken ct) =>
            Task.FromResult(new PayoutResult(id, Status, 500, reference.ToString("N")));
    }

    private sealed class Fixture : IDisposable
    {
        public SubscriptionDbContext Subscriptions { get; } =
            new(Options<SubscriptionDbContext>(), new Actor());
        public AccountsDbContext Accounts { get; } = new(Options<AccountsDbContext>(), new Actor());
        public WalletDbContext Wallets { get; } = new(Options<WalletDbContext>(), new Actor());
        public TradingDbContext Trading { get; } = FullLifecycleTests.Trading();
        public Quotes Quotes { get; } = new();
        public Guid VersionId { get; } = Guid.NewGuid();
        public AccountService AccountReader => new(Accounts);
        public WalletProvisioner WalletProvisioner => new(Wallets);
        public LiveEntitlementReader Entitlements => new(Subscriptions);
        public SubscriptionPurchaseService Purchases => new(Subscriptions);
        public EvaluationAccountProvisioner Provisioner =>
            new(Accounts, WalletProvisioner, new Policies());
        public PaidSubscriptionActivator Activator => new(Subscriptions, Provisioner);
        public PaperTradingEngine Paper =>
            new(
                Trading,
                AccountReader,
                new TradingEligibilityService(Subscriptions),
                new EvaluationTermsReader(Subscriptions),
                Quotes,
                new Risk(),
                new Actor(),
                new Instruments()
            );
        public ChallengeProgressionService Progression =>
            new(Subscriptions, Paper, Provisioner, new AccountLifecycle(Accounts));

        public async Task<UserSubscription> PurchaseAsync()
        {
            var version = new PlanVersion
            {
                PlanVersionId = VersionId,
                PlanInternalId = 1,
                ChallengeCapital = 10000,
                Status = "Active",
                EffectiveFrom = DateTimeOffset.UtcNow.AddDays(-1),
            };
            Subscriptions.Add(version);
            await Subscriptions.SaveChangesAsync();
            for (var stage = 1; stage <= 2; stage++)
                Subscriptions.Add(
                    new PlanStageDefinition
                    {
                        StageId = Guid.NewGuid(),
                        PlanVersionInternalId = version.Id,
                        StageNumber = stage,
                        StartingCapital = 10000,
                        ProfitTargetPercent = 8,
                        MinimumTradingDays = 1,
                        PolicySetId = Guid.NewGuid(),
                    }
                );
            await Subscriptions.SaveChangesAsync();
            return await Purchases.CreateAsync(1, VersionId, Guid.NewGuid(), null, default);
        }

        public void Dispose()
        {
            Subscriptions.Dispose();
            Accounts.Dispose();
            Wallets.Dispose();
            Trading.Dispose();
        }
    }
}
