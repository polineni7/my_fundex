namespace MyFundex.Trading;

public sealed record ProfitAllocation(
    decimal TraderGross,
    decimal Platform,
    decimal Tax,
    decimal Other,
    decimal Net
)
{
    public static ProfitAllocation Calculate(
        decimal profit,
        decimal share,
        decimal tax,
        decimal other
    )
    {
        if (share is <= 0 or > 100 || tax < 0 || other < 0 || tax + other >= 100)
            throw new ArgumentException("Invalid profit allocation percentages.");
        var available = decimal.Floor(Math.Max(0, profit) * 100m) / 100m;
        var gross = decimal.Floor(available * share) / 100m;
        var withheld = decimal.Floor(gross * tax) / 100m;
        var deduction = decimal.Floor(gross * other) / 100m;
        return new(gross, available - gross, withheld, deduction, gross - withheld - deduction);
    }
}
