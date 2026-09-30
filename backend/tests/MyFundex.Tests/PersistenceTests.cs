using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using MyFundex.BuildingBlocks.Abstractions;
using MyFundex.Identity;
using MyFundex.Wallet;
using Xunit;

namespace MyFundex.Tests;

public sealed class PersistenceTests
{
    private static readonly IDataProtectionProvider Protection =
        new EphemeralDataProtectionProvider();

    [Fact]
    public async Task SoftDeleteHidesRecordsAndIncrementsVersion()
    {
        using var db = new IdentityDbContext(
            new DbContextOptionsBuilder<IdentityDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options,
            new Actor(),
            Protection
        );
        var user = new User { UserId = Guid.NewGuid(), Email = "test@example.com" };
        db.Add(user);
        await db.SaveChangesAsync(true, default);
        Assert.Equal(42, user.CreatedBy);
        Assert.Equal(1, user.Version);
        db.Remove(user);
        await db.SaveChangesAsync();
        Assert.Empty(await db.Users.ToListAsync());
        var deleted = await db.Users.IgnoreQueryFilters().SingleAsync();
        Assert.True(deleted.IsDeleted);
        Assert.Equal(2, deleted.Version);
        Assert.Equal(42, deleted.DeletedBy);
    }

    [Fact]
    public async Task LedgerRowsCannotBeRewritten()
    {
        using var db = new WalletDbContext(
            new DbContextOptionsBuilder<WalletDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options,
            new Actor()
        );
        var entry = new LedgerEntry { EntryId = Guid.NewGuid(), Amount = 100 };
        db.Add(entry);
        await db.SaveChangesAsync();
        entry.Amount = 200;
        await Assert.ThrowsAsync<InvalidOperationException>(() => db.SaveChangesAsync());
    }

    [Fact]
    public async Task StaleUpdatesAreRejected()
    {
        var options = new DbContextOptionsBuilder<IdentityDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        using var first = new IdentityDbContext(options, new Actor(), Protection);
        first.Add(new User { UserId = Guid.NewGuid(), Email = "test@example.com" });
        await first.SaveChangesAsync();
        using var second = new IdentityDbContext(options, new Actor(), Protection);
        var stale = await second.Users.SingleAsync();
        var current = await first.Users.SingleAsync();
        current.FirstName = "First";
        await first.SaveChangesAsync();
        stale.FirstName = "Second";
        await Assert.ThrowsAsync<DbUpdateConcurrencyException>(() => second.SaveChangesAsync());
    }

    private sealed class Actor : ICurrentActor
    {
        public long ActorId => 42;
    }
}
