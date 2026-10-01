namespace MyFundex.Contracts;

public sealed record EvaluationTerms(
    decimal Capital,
    decimal ProfitTargetPercent,
    decimal DailyLossPercent,
    decimal TotalLossPercent,
    int MinimumDays,
    int? MaximumDays,
    DateTimeOffset StartedAt
);

public sealed record EvaluationSnapshot(
    Guid AccountId,
    string Status,
    decimal Cash,
    decimal Equity,
    decimal RealizedProfit,
    decimal DayOpeningEquity,
    int TradingDays,
    bool HasOpenPositions,
    DateTimeOffset ValuedAt
);

public interface IEvaluationTermsReader
{
    Task<EvaluationTerms?> GetAsync(Guid accountId, CancellationToken ct);
}

public interface IEvaluationProgressReader
{
    Task<EvaluationSnapshot?> GetAsync(Guid accountId, CancellationToken ct);
}

public interface IAccountLifecycle
{
    Task CloseEvaluationAsync(Guid accountId, string outcome, CancellationToken ct);
}

public sealed record LiveEntitlement(
    Guid SubscriptionId,
    long SubscriptionInternalId,
    long UserId,
    decimal Capital,
    Guid PolicySetId,
    decimal RewardSharePercent,
    decimal DailyLossPercent = 5,
    decimal TotalLossPercent = 10,
    decimal TaxWithholdingPercent = 0,
    decimal OtherDeductionPercent = 0
);

public interface ILiveEntitlementReader
{
    Task<LiveEntitlement?> GetAsync(Guid subscriptionId, CancellationToken ct);
}

public interface IBrokerAccountVerifier
{
    Task<bool> VerifyAsync(
        string provider,
        string credentialKey,
        string brokerUserId,
        CancellationToken ct
    );
}

public interface ILiveAccountProvisioner
{
    Task<Guid> ProvisionAsync(
        Guid subscriptionId,
        string provider,
        string credentialKey,
        string brokerUserId,
        CancellationToken ct
    );
}

public interface ILiveRiskTermsReader
{
    Task<LiveEntitlement?> GetByAccountAsync(Guid accountId, CancellationToken ct);
}
