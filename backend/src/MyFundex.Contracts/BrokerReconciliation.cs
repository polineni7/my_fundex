namespace MyFundex.Contracts;

public sealed record BrokerFill(
    string TradeId,
    decimal Quantity,
    decimal Price,
    DateTimeOffset ExecutedAt
);

public sealed record BrokerObservation(
    string OrderId,
    string Status,
    string InstrumentToken,
    string Side,
    decimal Quantity,
    decimal FilledQuantity,
    IReadOnlyList<BrokerFill> Fills
);

public interface IBrokerOrderReader
{
    Task<BrokerObservation?> ReadAsync(
        string provider,
        string credentialKey,
        string? orderId,
        string tag,
        CancellationToken ct
    );
}

public interface IBrokerReconciliationAdapter : IBrokerOrderReader
{
    string Provider { get; }
}

public interface IBrokerVerificationAdapter : IBrokerAccountVerifier
{
    string Provider { get; }
}
