using Microsoft.EntityFrameworkCore;
using MyFundex.Broker;
using MyFundex.BuildingBlocks.Abstractions;
using MyFundex.Contracts;
using MyFundex.Subscription;
using Xunit;

namespace MyFundex.Tests;

public sealed class TradingLifecycleTests
{
    [Fact]
    public async Task CancellationIsOwnedAndNeverClaimsFinalBrokerSettlement()
    {
        using var db = new MyFundex.Trading.TradingDbContext(
            new DbContextOptionsBuilder<MyFundex.Trading.TradingDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options,
            new Actor()
        );
        var order = new MyFundex.Trading.Order
        {
            OrderId = Guid.NewGuid(),
            AccountId = Guid.NewGuid(),
            Status = "Submitted",
            BrokerOrderId = "broker-order",
        };
        db.Add(order);
        await db.SaveChangesAsync();
        var adapter = new Adapter();
        var registry = new BrokerAdapterRegistry([adapter]);
        var gateway = new TradingGateway(new(registry), new(registry), registry);
        var denied = new MyFundex.Trading.CancelOrderService(
            db,
            new Accounts(2),
            gateway,
            new Actor()
        );
        Assert.False((await denied.CancelAsync(order.OrderId, default)).Success);
        Assert.Equal("Submitted", order.Status);
        var service = new MyFundex.Trading.CancelOrderService(
            db,
            new Accounts(1),
            gateway,
            new Actor()
        );
        Assert.True((await service.CancelAsync(order.OrderId, default)).Success);
        Assert.Equal("CancellationPendingReconciliation", order.Status);
        await service.CancelAsync(order.OrderId, default);
        Assert.Single(await db.OrderEvents.ToListAsync());
    }

    private sealed class Accounts(long owner) : IFundedAccountReader
    {
        public Task<FundedAccountSnapshot?> GetByPublicIdAsync(
            Guid accountId,
            CancellationToken ct
        ) =>
            Task.FromResult<FundedAccountSnapshot?>(
                new(1, accountId, owner, 1000, 900, "Active", 1)
            );
    }

    [Fact]
    public async Task SandboxAndProductionCanRouteConcurrentlyWithoutSharingCredentials()
    {
        var adapter = new Adapter();
        var registry = new BrokerAdapterRegistry([adapter]);
        var gateway = new TradingGateway(new(registry), new(registry), registry);
        var sandbox = new BrokerOrderRequest("instrument", "BUY", 1, "LIMIT", 100, "paper");
        var live = sandbox with
        {
            Environment = "PRODUCTION",
            CredentialKey = "account-a",
            Tag = "live",
        };
        await Task.WhenAll(gateway.PlaceAsync(sandbox, default), gateway.PlaceAsync(live, default));
        Assert.Contains(
            adapter.Requests,
            x => x.Environment == "SANDBOX" && x.CredentialKey == null
        );
        Assert.Contains(
            adapter.Requests,
            x => x.Environment == "PRODUCTION" && x.CredentialKey == "account-a"
        );
        await Assert.ThrowsAsync<ArgumentException>(
            () => gateway.PlaceAsync(live with { CredentialKey = null }, default)
        );
        await Assert.ThrowsAsync<ArgumentException>(
            () => gateway.PlaceAsync(sandbox with { Provider = "Unknown" }, default)
        );
    }

    [Fact]
    public async Task PurchaseRetriesReuseSubscriptionAndRepurchasePreservesFailedHistory()
    {
        using var db = Context();
        var version = new PlanVersion
        {
            PlanVersionId = Guid.NewGuid(),
            PlanInternalId = 5,
            Status = "Active",
            EffectiveFrom = DateTimeOffset.UtcNow.AddDays(-1),
        };
        db.Add(version);
        await db.SaveChangesAsync();
        var service = new SubscriptionPurchaseService(db);
        var key = Guid.NewGuid();
        var first = await service.CreateAsync(1, version.PlanVersionId, key, null, default);
        var repeated = await service.CreateAsync(1, version.PlanVersionId, key, null, default);
        Assert.Equal(first.SubscriptionId, repeated.SubscriptionId);
        await Assert.ThrowsAsync<ArgumentException>(
            () => service.CreateAsync(2, version.PlanVersionId, key, null, default)
        );
        await Assert.ThrowsAsync<ArgumentException>(
            () =>
                service.CreateAsync(
                    1,
                    version.PlanVersionId,
                    Guid.NewGuid(),
                    first.SubscriptionId,
                    default
                )
        );
        first.Status = "Failed";
        await db.SaveChangesAsync();
        var next = await service.CreateAsync(
            1,
            version.PlanVersionId,
            Guid.NewGuid(),
            first.SubscriptionId,
            default
        );
        Assert.NotEqual(first.SubscriptionId, next.SubscriptionId);
        Assert.Equal("PendingPayment", next.Status);
        Assert.Null(next.ActivatedAt);
        Assert.Equal("Failed", first.Status);
        Assert.Equal(first.SubscriptionId, next.PreviousSubscriptionId);
    }

    [Fact]
    public async Task OnlyOwnersActivePaidAttemptIsEligible()
    {
        using var db = Context();
        var subscription = new UserSubscription
        {
            SubscriptionId = Guid.NewGuid(),
            UserInternalId = 1,
            Status = "Evaluation",
            ActivatedAt = DateTimeOffset.UtcNow,
        };
        db.Add(subscription);
        await db.SaveChangesAsync();
        var accountId = Guid.NewGuid();
        db.Add(
            new ChallengeAttempt
            {
                AttemptId = Guid.NewGuid(),
                AccountId = accountId,
                SubscriptionInternalId = subscription.Id,
                Status = "InProgress",
            }
        );
        await db.SaveChangesAsync();
        var service = new TradingEligibilityService(db);
        Assert.True((await service.CheckAsync(accountId, 1, "Evaluation", default)).Allowed);
        Assert.False((await service.CheckAsync(accountId, 2, "Evaluation", default)).Allowed);
        Assert.False((await service.CheckAsync(accountId, 1, "Funded", default)).Allowed);
        subscription.Status = "Failed";
        await db.SaveChangesAsync();
        Assert.False((await service.CheckAsync(accountId, 1, "Evaluation", default)).Allowed);
    }

    private static SubscriptionDbContext Context() =>
        new(
            new DbContextOptionsBuilder<SubscriptionDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options,
            new Actor()
        );

    private sealed class Actor : ICurrentActor
    {
        public long ActorId => 1;
    }

    private sealed class Adapter : IBrokerAdapter
    {
        public string Provider => "Upstox";
        public List<BrokerOrderRequest> Requests { get; } = [];

        public Task<BrokerOrderResponse> PlaceAsync(
            BrokerOrderRequest request,
            CancellationToken ct
        )
        {
            Requests.Add(request);
            return Task.FromResult(new BrokerOrderResponse(true, request.Tag, null, null, ""));
        }

        public Task<BrokerCancellation> CancelAsync(
            BrokerOrderReference order,
            CancellationToken ct
        ) => Task.FromResult(new BrokerCancellation(true, null));
    }
}
