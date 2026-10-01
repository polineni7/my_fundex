using MyFundex.Subscription;
using Xunit;

namespace MyFundex.Tests;

public sealed class PlanConfigurationTests
{
    [Theory]
    [InlineData(1, 7)]
    [InlineData(2, 14)]
    [InlineData(3, 21)]
    [InlineData(4, 29)]
    [InlineData(5, 60)]
    [InlineData(6, 90)]
    [InlineData(7, 182)]
    [InlineData(8, 366)]
    public void PeriodsUseCalendarMonthsIncludingLeapYears(int period, int days)
    {
        Assert.Equal(days, TradingPeriods.CalendarDays(period, null, new DateTimeOffset(2024, 1, 31, 0, 0, 0, TimeSpan.Zero)));
    }

    [Fact]
    public void UnlimitedLegacyAndZeroAreNotConfused()
    {
        Assert.Null(TradingPeriods.CalendarDays(0, null, DateTimeOffset.UtcNow));
        Assert.Equal(42, TradingPeriods.CalendarDays(null, 42, DateTimeOffset.UtcNow));
        Assert.Throws<ArgumentException>(() => TradingPeriods.Validate(null, 0, null));
        Assert.Throws<ArgumentException>(() => TradingPeriods.Validate(4, 30, null));
        Assert.Throws<ArgumentException>(() => TradingPeriods.Validate(99, null, null));
        TradingPeriods.Validate(null, null, null);
        TradingPeriods.Validate(0, null, 100);
        Assert.Throws<ArgumentException>(() => TradingPeriods.Validate(0, null, 0));
        Assert.Throws<ArgumentException>(() => TradingPeriods.Validate(0, null, 1.5m));
    }

    [Fact]
    public void OptionalMinimumDaysAndExplicitZeroAreAcceptedButNegativeIsNot()
    {
        ChallengeEvaluator.Validate(new(8, 5, 10, null, null));
        ChallengeEvaluator.Validate(new(8, 5, 10, 0, null));
        Assert.Throws<ArgumentException>(() => ChallengeEvaluator.Validate(new(8, 5, 10, -1, null)));
        var stage = new PlanStageDefinition { MinimumTradingDays = null, MaximumLeverage = null };
        Assert.Null(stage.MinimumTradingDays);
        stage.MinimumTradingDays = 0;
        Assert.Equal(0, stage.MinimumTradingDays);
    }
}
