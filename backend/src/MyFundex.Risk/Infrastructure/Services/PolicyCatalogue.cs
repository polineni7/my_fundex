using Microsoft.EntityFrameworkCore;
using MyFundex.Contracts;

namespace MyFundex.Risk;

public sealed class PolicyCatalogue(RiskDbContext db) : IPolicyCatalogue
{
    public Task<bool> IsActiveAsync(Guid policySetId, CancellationToken ct)
    {
        var now = DateTimeOffset.UtcNow;
        return (
            from policy in db.PolicySets.AsNoTracking()
            join version in db.PolicyVersions.AsNoTracking()
                on policy.Id equals version.PolicySetInternalId
            where
                policy.PolicyId == policySetId
                && version.Status == "Active"
                && version.EffectiveFrom <= now
                && (version.EffectiveTo == null || version.EffectiveTo > now)
            select version.Id
        ).AnyAsync(ct);
    }
}
