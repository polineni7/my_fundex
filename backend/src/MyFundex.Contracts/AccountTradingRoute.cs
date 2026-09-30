namespace MyFundex.Contracts;

public static class AccountTradingRoute
{
    public static string Resolve(string mode) =>
        mode switch
        {
            "Evaluation" => "SANDBOX",
            "Funded" => "PRODUCTION",
            _ => throw new InvalidOperationException("Account is not enabled for trading."),
        };
}
