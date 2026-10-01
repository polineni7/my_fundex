namespace MyFundex.Subscription;

public sealed record TradingPeriodOption(int Value, string Label);

public static class TradingPeriods
{
    // Stable API identifiers. Months are calendar months, not fixed 30-day intervals.
    public static readonly TradingPeriodOption[] Options =
    [
        new(0, "Unlimited"), new(1, "1 week"), new(2, "2 weeks"), new(3, "3 weeks"),
        new(4, "1 month"), new(5, "2 months"), new(6, "3 months"),
        new(7, "6 months"), new(8, "1 year")
    ];

    public static string Label(int? period, int? legacyDays) => period.HasValue
        ? Options.Single(x => x.Value == period).Label
        : legacyDays.HasValue ? $"{legacyDays} calendar days" : "Unlimited";

    public static void Validate(int? period, int? legacyDays, decimal? maximumLeverage)
    {
        if (period is < 0 or > 8 || legacyDays is <= 0)
            throw new ArgumentException("Choose a valid trading period. Zero calendar days is not unlimited.");
        if (period.HasValue && legacyDays.HasValue)
            throw new ArgumentException("Choose a trading period or custom calendar days, not both.");
        if (maximumLeverage is < 1 or > 100 || maximumLeverage.HasValue && decimal.Truncate(maximumLeverage.Value) != maximumLeverage)
            throw new ArgumentException("Maximum leverage must be a whole number from 1 to 100, or left blank.");
    }

    public static int? CalendarDays(int? period, int? legacyDays, DateTimeOffset startedAt)
    {
        Validate(period, legacyDays, null);
        var start = DateOnly.FromDateTime(startedAt.ToOffset(TimeSpan.FromHours(5.5)).DateTime);
        return period switch
        {
            null => legacyDays,
            0 => null,
            1 => 7,
            2 => 14,
            3 => 21,
            4 => start.AddMonths(1).DayNumber - start.DayNumber,
            5 => start.AddMonths(2).DayNumber - start.DayNumber,
            6 => start.AddMonths(3).DayNumber - start.DayNumber,
            7 => start.AddMonths(6).DayNumber - start.DayNumber,
            8 => start.AddYears(1).DayNumber - start.DayNumber,
            _ => throw new ArgumentException("Unknown trading period.")
        };
    }
}
