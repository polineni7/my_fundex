namespace MyFundex.Subscription;

public sealed record ChallengeRules(
    decimal ProfitTargetPercent,
    decimal MaxDailyLossPercent,
    decimal MaxTotalLossPercent,
    int MinimumTradingDays,
    int? MaximumCalendarDays
);

public sealed record ChallengeProgress(
    decimal StartingCapital,
    decimal RealizedProfit,
    decimal CurrentEquity,
    decimal DayStartingEquity,
    int TradingDays,
    int ElapsedCalendarDays,
    bool HasOpenPositions,
    bool HasPendingOrders
);

public sealed record ChallengeDecision(string Status, string Reason);

public static class ChallengeEvaluator
{
    public static void Validate(ChallengeRules rules)
    {
        if (
            rules.ProfitTargetPercent <= 0
            || rules.ProfitTargetPercent > 100
            || rules.MaxDailyLossPercent <= 0
            || rules.MaxDailyLossPercent > 100
            || rules.MaxTotalLossPercent <= 0
            || rules.MaxTotalLossPercent > 100
            || rules.MinimumTradingDays < 1
            || rules.MaximumCalendarDays is <= 0
        )
            throw new ArgumentException(
                "Challenge targets, loss limits and trading days must be positive and within range."
            );
    }

    public static ChallengeDecision Evaluate(ChallengeRules rules, ChallengeProgress progress)
    {
        Validate(rules);
        if (
            progress.StartingCapital <= 0
            || progress.DayStartingEquity <= 0
            || progress.TradingDays < 0
            || progress.ElapsedCalendarDays < 0
        )
            throw new ArgumentException("Challenge progress is invalid.");
        var totalLoss = progress.StartingCapital - progress.CurrentEquity;
        var dailyLoss = progress.DayStartingEquity - progress.CurrentEquity;
        if (totalLoss >= progress.StartingCapital * rules.MaxTotalLossPercent / 100m)
            return new("Failed", "Maximum total loss reached.");
        if (dailyLoss >= progress.DayStartingEquity * rules.MaxDailyLossPercent / 100m)
            return new("Failed", "Maximum daily loss reached.");
        if (rules.MaximumCalendarDays is int maximum && progress.ElapsedCalendarDays > maximum)
            return new("Failed", "Evaluation period expired.");
        if (progress.RealizedProfit < progress.StartingCapital * rules.ProfitTargetPercent / 100m)
            return new("InProgress", "Profit target not reached.");
        if (progress.TradingDays < rules.MinimumTradingDays)
            return new("InProgress", "Minimum trading days not reached.");
        if (progress.HasOpenPositions || progress.HasPendingOrders)
            return new(
                "InProgress",
                "Close positions and resolve pending orders before progressing."
            );
        return new("Passed", "All stage objectives satisfied.");
    }
}
