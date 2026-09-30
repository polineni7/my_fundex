using Microsoft.EntityFrameworkCore;
using MyFundex.BuildingBlocks.Abstractions;
using MyFundex.FundedAccounts;
using MyFundex.Subscription;
using Xunit;

namespace MyFundex.Tests;

public sealed class RegressionTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(-100)]
    public async Task CapitalChangesRejectNonPositiveAmounts(decimal amount)
    {
        using var db = new AccountsDbContext(
            new DbContextOptionsBuilder<AccountsDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options,
            new Actor()
        );
        var service = new AccountService(db);
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
            () => service.TryReserveAsync(1, amount, 1, default)
        );
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
            () => service.ReleaseAsync(1, amount, default)
        );
    }

    [Theory]
    [InlineData("Cancelled")]
    [InlineData("Failed")]
    public async Task InvalidSubscriptionStatesCannotBeMarkedFulfilled(string status)
    {
        using var db = new SubscriptionDbContext(
            new DbContextOptionsBuilder<SubscriptionDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options,
            new Actor()
        );
        var id = Guid.NewGuid();
        db.Add(
            new UserSubscription
            {
                SubscriptionId = id,
                UserInternalId = 1,
                Status = status,
            }
        );
        await db.SaveChangesAsync();
        var activator = new PaidSubscriptionActivator(db, null!);
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => activator.ActivateAsync(id, 1, default)
        );
    }

    private sealed class Actor : ICurrentActor
    {
        public long ActorId => 1;
    }
}
