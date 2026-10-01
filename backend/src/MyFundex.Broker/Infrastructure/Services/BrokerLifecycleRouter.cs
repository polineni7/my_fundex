using MyFundex.Contracts;

namespace MyFundex.Broker;

public sealed class BrokerLifecycleRouter(
    IEnumerable<IBrokerReconciliationAdapter> readers,
    IEnumerable<IBrokerVerificationAdapter> verifiers
) : IBrokerOrderReader, IBrokerAccountVerifier
{
    private readonly IReadOnlyDictionary<string, IBrokerReconciliationAdapter> orderReaders =
        readers.ToDictionary(x => x.Provider, StringComparer.OrdinalIgnoreCase);
    private readonly IReadOnlyDictionary<string, IBrokerVerificationAdapter> accountVerifiers =
        verifiers.ToDictionary(x => x.Provider, StringComparer.OrdinalIgnoreCase);

    public Task<BrokerObservation?> ReadAsync(
        string provider,
        string credentialKey,
        string? orderId,
        string tag,
        CancellationToken ct
    ) =>
        orderReaders.TryGetValue(provider, out var adapter)
            ? adapter.ReadAsync(provider, credentialKey, orderId, tag, ct)
            : throw new ArgumentException("Provider reconciliation adapter is not configured.");

    public Task<bool> VerifyAsync(
        string provider,
        string credentialKey,
        string brokerUserId,
        CancellationToken ct
    ) =>
        accountVerifiers.TryGetValue(provider, out var adapter)
            ? adapter.VerifyAsync(provider, credentialKey, brokerUserId, ct)
            : throw new ArgumentException(
                "Provider account verification adapter is not configured."
            );
}
