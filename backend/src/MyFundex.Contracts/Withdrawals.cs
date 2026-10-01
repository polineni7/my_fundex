namespace MyFundex.Contracts;

public interface IWithdrawalFunds
{
    Task ReserveAsync(Guid withdrawalId, long accountId, decimal amount, CancellationToken ct);
    Task CompleteAsync(Guid withdrawalId, bool paid, CancellationToken ct);
}

public sealed record PayoutResult(string Id, string Status, decimal Amount, string ReferenceId);

public interface IPayoutGateway
{
    Task<PayoutResult> SendAsync(
        Guid withdrawalId,
        string beneficiary,
        decimal amount,
        CancellationToken ct
    );
    Task<PayoutResult> ReadAsync(string payoutId, CancellationToken ct);
}
