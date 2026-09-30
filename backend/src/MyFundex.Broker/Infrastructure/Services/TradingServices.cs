using MyFundex.Contracts;

namespace MyFundex.Broker;

public sealed class BrokerAdapterRegistry(IEnumerable<IBrokerAdapter> adapters)
{
    private readonly IReadOnlyDictionary<string, IBrokerAdapter> providers = adapters.ToDictionary(
        adapter => adapter.Provider,
        StringComparer.OrdinalIgnoreCase
    );

    public IBrokerAdapter Get(string provider) =>
        providers.TryGetValue(provider, out var adapter)
            ? adapter
            : throw new ArgumentException("Trading provider is not configured.");
}

public sealed class PaperTradingService(BrokerAdapterRegistry providers)
{
    public Task<BrokerOrderResponse> PlaceAsync(BrokerOrderRequest request, CancellationToken ct)
    {
        if (request.Environment != "SANDBOX")
            throw new ArgumentException("Paper trading requires the sandbox environment.");
        return providers.Get(request.Provider).PlaceAsync(request, ct);
    }
}

public sealed class ProductionTradingService(BrokerAdapterRegistry providers)
{
    public Task<BrokerOrderResponse> PlaceAsync(BrokerOrderRequest request, CancellationToken ct)
    {
        if (request.Environment != "PRODUCTION" || string.IsNullOrWhiteSpace(request.CredentialKey))
            throw new ArgumentException(
                "Real trading requires an explicitly linked broker account."
            );
        return providers.Get(request.Provider).PlaceAsync(request, ct);
    }
}

public sealed class TradingGateway(
    PaperTradingService paper,
    ProductionTradingService production,
    BrokerAdapterRegistry providers
) : IBrokerOrderGateway, IOrderCancellationGateway
{
    public Task<BrokerOrderResponse> PlaceAsync(BrokerOrderRequest request, CancellationToken ct) =>
        request.Environment switch
        {
            "SANDBOX" => paper.PlaceAsync(request, ct),
            "PRODUCTION" => production.PlaceAsync(request, ct),
            _ => throw new ArgumentException("Invalid trading environment."),
        };

    public Task<BrokerCancellation> CancelAsync(BrokerOrderReference order, CancellationToken ct)
    {
        if (order.Environment is not ("SANDBOX" or "PRODUCTION"))
            throw new ArgumentException("Invalid trading environment.");
        if (order.Environment == "PRODUCTION" && string.IsNullOrWhiteSpace(order.CredentialKey))
            throw new ArgumentException("Real trading requires a linked broker account.");
        return providers.Get(order.Provider).CancelAsync(order, ct);
    }
}
