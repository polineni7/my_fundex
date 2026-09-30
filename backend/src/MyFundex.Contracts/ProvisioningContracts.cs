namespace MyFundex.Contracts;

public sealed record EvaluationAccountRequest(
    Guid ProvisioningId,
    long UserId,
    long SubscriptionInternalId,
    decimal Capital,
    Guid PolicySetId
);

public interface IEvaluationAccountProvisioner
{
    Task<Guid> ProvisionAsync(EvaluationAccountRequest request, CancellationToken ct);
}

public interface IWalletProvisioner
{
    Task EnsureAsync(long accountInternalId, CancellationToken ct);
}

public interface IPolicyAssignmentService
{
    Task AssignActiveAsync(long accountInternalId, Guid policySetId, CancellationToken ct);
}

public interface IPaidSubscriptionActivator
{
    Task ActivateAsync(Guid subscriptionId, long userId, CancellationToken ct);
}
