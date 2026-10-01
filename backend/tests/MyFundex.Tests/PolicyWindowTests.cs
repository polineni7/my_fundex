using Microsoft.EntityFrameworkCore;
using MyFundex.BuildingBlocks.Abstractions;
using MyFundex.Contracts;
using MyFundex.Risk;
using Xunit;

namespace MyFundex.Tests;

public sealed class PolicyWindowTests
{
    [Theory]
    [InlineData(3, 44, false)]
    [InlineData(3, 45, true)]
    [InlineData(9, 29, true)]
    [InlineData(9, 30, false)]
    public void EntryWindowUsesIndiaTimeAndExclusiveEnd(int hour, int minute, bool allowed)
    {
        var now = new DateTimeOffset(2026, 10, 1, hour, minute, 0, TimeSpan.Zero);
        Assert.Equal(allowed, TradingWindow.Allows("09:15|15:00", "BUY", now));
        Assert.True(TradingWindow.Allows("09:15|15:00", "SELL", now));
    }

    [Fact]
    public void WindowRequiresBothValidTimes()
    {
        Assert.Null(TradingWindow.Encode(null, null));
        Assert.Throws<ArgumentException>(() => TradingWindow.Encode("09:15", null));
        Assert.Throws<ArgumentException>(() => TradingWindow.Encode("15:00", "09:15"));
        Assert.Throws<ArgumentException>(() => TradingWindow.Encode("25:00", "26:00"));
        Assert.Throws<ArgumentException>(() => TradingWindow.Allows(null, "BUY", DateTimeOffset.UtcNow));
    }

    [Fact]
    public async Task AssignedWindowRejectsAndAuditsEntryButAllowsExit()
    {
        await using var db = new RiskDbContext(new DbContextOptionsBuilder<RiskDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options, new Actor());
        var now = DateTimeOffset.UtcNow;
        var india = now.ToOffset(TimeSpan.FromHours(5.5));
        var window = india.Hour < 12 ? "20:00|21:00" : "01:00|02:00";
        var version = new PolicyVersion { Status = "Active", EffectiveFrom = now.AddDays(-1) };
        db.Add(version); await db.SaveChangesAsync();
        db.Add(new PolicyAssignment { FundedAccountInternalId = 42, PolicyVersionInternalId = version.Id, EffectiveFrom = now.AddDays(-1) });
        db.Add(new PolicyRule { RuleCode = "ENTRY_TIME_WINDOW", StringValue = window, PolicyVersionInternalId = version.Id });
        await db.SaveChangesAsync();
        var request = new RiskCheckRequest(42, Guid.NewGuid(), "NSE_EQ|TEST", "BUY", 1, 100, 100);
        var guard = new RiskGuard(db);
        Assert.False((await guard.EvaluateAsync(request, default)).Allowed);
        Assert.Single(await db.Violations.ToListAsync());
        Assert.True((await guard.EvaluateAsync(request with { Side = "SELL" }, default)).Allowed);
        Assert.Single(await db.Violations.ToListAsync());
    }
    private sealed class Actor : ICurrentActor { public long ActorId => 0; }
}
