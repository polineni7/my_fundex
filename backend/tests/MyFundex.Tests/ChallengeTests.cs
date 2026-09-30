using MyFundex.Contracts;
using MyFundex.Subscription;
using Xunit;

namespace MyFundex.Tests;

public sealed class ChallengeTests
{
    private static readonly ChallengeRules Rules = new(8m, 5m, 10m, 5, null);
    private static readonly ChallengeProgress Progress = new(
        100_000m,
        8_000m,
        108_000m,
        108_000m,
        5,
        10,
        false,
        false
    );

    [Fact]
    public void AllObjectivesAreRequiredToPass()
    {
        Assert.Equal("Passed", ChallengeEvaluator.Evaluate(Rules, Progress).Status);
        Assert.Equal(
            "InProgress",
            ChallengeEvaluator.Evaluate(Rules, Progress with { TradingDays = 4 }).Status
        );
        Assert.Equal(
            "InProgress",
            ChallengeEvaluator.Evaluate(Rules, Progress with { HasOpenPositions = true }).Status
        );
        Assert.Equal(
            "InProgress",
            ChallengeEvaluator.Evaluate(Rules, Progress with { HasPendingOrders = true }).Status
        );
        Assert.Equal(
            "InProgress",
            ChallengeEvaluator.Evaluate(Rules, Progress with { RealizedProfit = 7_999m }).Status
        );
    }

    [Fact]
    public void LossAtThresholdFailsEvenWhenProfitTargetWasReached()
    {
        Assert.Equal(
            "Failed",
            ChallengeEvaluator.Evaluate(Rules, Progress with { CurrentEquity = 90_000m }).Status
        );
        Assert.Equal(
            "Failed",
            ChallengeEvaluator
                .Evaluate(
                    Rules,
                    Progress with
                    {
                        DayStartingEquity = 100_000m,
                        CurrentEquity = 95_000m,
                    }
                )
                .Status
        );
    }

    [Fact]
    public void UnlimitedDurationAndExpiryAreDistinct()
    {
        Assert.Equal(
            "Passed",
            ChallengeEvaluator.Evaluate(Rules, Progress with { ElapsedCalendarDays = 999 }).Status
        );
        Assert.Equal(
            "Failed",
            ChallengeEvaluator.Evaluate(Rules with { MaximumCalendarDays = 9 }, Progress).Status
        );
    }

    [Fact]
    public void InvalidRulesCannotPass()
    {
        Assert.Throws<ArgumentException>(
            () => ChallengeEvaluator.Evaluate(Rules with { MaxDailyLossPercent = 0 }, Progress)
        );
        Assert.Throws<ArgumentException>(
            () => ChallengeEvaluator.Evaluate(Rules, Progress with { StartingCapital = 0 })
        );
    }

    [Fact]
    public void EvaluationCannotRouteToProduction()
    {
        Assert.Equal("SANDBOX", AccountTradingRoute.Resolve("Evaluation"));
        Assert.Equal("PRODUCTION", AccountTradingRoute.Resolve("Funded"));
        Assert.Throws<InvalidOperationException>(
            () => AccountTradingRoute.Resolve("AwaitingBrokerLink")
        );
    }
}
