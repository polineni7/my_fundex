namespace MyFundex.Contracts;

public sealed record TradingEligibility(bool Allowed, string? Reason = null);

public interface ITradingEligibility
{
    Task<TradingEligibility> CheckAsync(
        Guid accountId,
        long userId,
        string mode,
        CancellationToken ct
    );
}

public sealed record BrokerOrderReference(
    string Provider,
    string Environment,
    string? CredentialKey,
    string OrderId
);

public sealed record BrokerCancellation(bool Accepted, string? Error);

/// <summary>One adapter per provider. Environment is selected by the application, never the browser.</summary>
public interface IBrokerAdapter
{
    string Provider { get; }
    Task<BrokerOrderResponse> PlaceAsync(BrokerOrderRequest request, CancellationToken ct);
    Task<BrokerCancellation> CancelAsync(BrokerOrderReference order, CancellationToken ct);
}

public interface IOrderCancellationGateway
{
    Task<BrokerCancellation> CancelAsync(BrokerOrderReference order, CancellationToken ct);
}
