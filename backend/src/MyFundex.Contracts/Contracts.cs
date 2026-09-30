namespace MyFundex.Contracts;

public sealed record FundedAccountSnapshot(
    long InternalId,
    Guid AccountId,
    long UserId,
    decimal FundedCapital,
    decimal BuyingPower,
    string Status,
    long Version,
    string TradingMode = "Evaluation",
    string? BrokerCredentialKey = null
);

public interface IFundedAccountReader
{
    Task<FundedAccountSnapshot?> GetByPublicIdAsync(Guid accountId, CancellationToken ct);
}

public interface IFundedAccountCapitalService
{
    Task<bool> TryReserveAsync(
        long accountInternalId,
        decimal amount,
        long expectedVersion,
        CancellationToken ct
    );
    Task ReleaseAsync(long accountInternalId, decimal amount, CancellationToken ct);
}

public sealed record RiskCheckRequest(
    long AccountInternalId,
    Guid AccountId,
    string InstrumentToken,
    string Side,
    decimal Quantity,
    decimal EstimatedPrice,
    decimal EstimatedValue
);

public sealed record RiskCheckResult(bool Allowed, string? Code = null, string? Message = null);

public interface IRiskGuard
{
    Task<RiskCheckResult> EvaluateAsync(RiskCheckRequest request, CancellationToken ct);
}

public sealed record BrokerOrderRequest(
    string InstrumentToken,
    string Side,
    decimal Quantity,
    string OrderType,
    decimal? Price,
    string Tag,
    string Environment = "SANDBOX",
    string? CredentialKey = null
);

public sealed record BrokerOrderResponse(
    bool Success,
    string? BrokerOrderId,
    string? ErrorCode,
    string? ErrorMessage,
    string RawPayload
);

public interface IBrokerOrderGateway
{
    Task<BrokerOrderResponse> PlaceAsync(BrokerOrderRequest request, CancellationToken ct);
}

public interface IMarketQuoteProvider
{
    Task<decimal?> GetLtpAsync(string instrumentToken, CancellationToken ct);
}

public sealed record WalletSnapshot(
    long WalletInternalId,
    Guid WalletId,
    long FundedAccountInternalId,
    decimal AvailableBalance,
    decimal WithdrawableBalance
);

public interface IWalletReader
{
    Task<WalletSnapshot?> GetByAccountAsync(long fundedAccountInternalId, CancellationToken ct);
}

public interface IWalletLedger
{
    Task CreditProfitAsync(
        long fundedAccountInternalId,
        decimal amount,
        string referenceType,
        string referenceId,
        CancellationToken ct
    );
}

public interface IAuditWriter
{
    Task WriteAsync(
        string module,
        string entityType,
        string entityId,
        string action,
        string? oldValue,
        string? newValue,
        CancellationToken ct
    );
}

public interface INotificationSender
{
    Task CreateAsync(long userId, string type, string title, string message, CancellationToken ct);
}

public sealed record IntegrationEvent(
    Guid EventId,
    string EventType,
    string AggregateType,
    string AggregateId,
    string Payload,
    DateTimeOffset OccurredAt
);

public interface IRuntimeSettings
{
    Task<string?> GetAsync(string key, string environment, CancellationToken ct);
    Task UpsertAsync(
        string key,
        string value,
        string environment,
        string category,
        long actor,
        CancellationToken ct
    );
}
