using Microsoft.EntityFrameworkCore;
using MyFundex.Contracts;

namespace MyFundex.Risk;

public sealed class PolicyAssignmentService(RiskDbContext db) : IPolicyAssignmentService
{
    public async Task AssignActiveAsync(
        long accountInternalId,
        Guid policySetId,
        CancellationToken ct
    )
    {
        if (await db.Assignments.AnyAsync(x => x.FundedAccountInternalId == accountInternalId, ct))
            return;
        var now = DateTimeOffset.UtcNow;
        var version =
            await (
                from policy in db.PolicySets.AsNoTracking()
                join candidate in db.PolicyVersions.AsNoTracking()
                    on policy.Id equals candidate.PolicySetInternalId
                where
                    policy.PolicyId == policySetId
                    && candidate.Status == "Active"
                    && candidate.EffectiveFrom <= now
                    && (candidate.EffectiveTo == null || candidate.EffectiveTo > now)
                orderby candidate.VersionNumber descending
                select candidate
            ).FirstOrDefaultAsync(ct)
            ?? throw new InvalidOperationException(
                "No active policy version is available for this stage."
            );
        db.Add(
            new PolicyAssignment
            {
                AssignmentId = Guid.NewGuid(),
                FundedAccountInternalId = accountInternalId,
                PolicyVersionInternalId = version.Id,
                EffectiveFrom = now,
            }
        );
        await db.SaveChangesAsync(ct);
    }
}
