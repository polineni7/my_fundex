namespace MyFundex.Payments;

public static class CouponPricing
{
    public static decimal Discount(string type, decimal value, decimal fee)
    {
        if (fee < 1 || value <= 0 || decimal.Round(value, 2) != value ||
            (type != "Percentage" && type != "Fixed") || (type == "Percentage" && value > 100))
            throw new ArgumentException("Invalid discount terms.");
        return Math.Min(fee - 1, decimal.Round(type == "Percentage" ? fee * value / 100 : value, 2, MidpointRounding.AwayFromZero));
    }
}
