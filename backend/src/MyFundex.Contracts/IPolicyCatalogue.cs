namespace MyFundex.Contracts;

public interface IPolicyCatalogue
{
    Task<bool> IsActiveAsync(Guid policySetId, CancellationToken ct);
}
