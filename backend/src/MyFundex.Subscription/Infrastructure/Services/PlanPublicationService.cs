using Microsoft.EntityFrameworkCore;
using MyFundex.Contracts;

namespace MyFundex.Subscription;

public sealed class PlanPublicationService(
    SubscriptionDbContext db,
    IPolicyCatalogue policies,
    IAuditWriter audit
)
{
    public async Task PublishAsync(Guid versionId, long expectedVersion, CancellationToken ct)
    {
        var version = await db.PlanVersions.SingleAsync(x => x.PlanVersionId == versionId, ct);
        if (version.Version != expectedVersion)
            throw new DbUpdateConcurrencyException();
        if (version.Status != "Draft")
            throw new ArgumentException("Only draft plan versions can be published.");
        var stages = await db
            .Stages.AsNoTracking()
            .Where(x => x.PlanVersionInternalId == version.Id)
            .OrderBy(x => x.StageNumber)
            .ToListAsync(ct);
        if (stages.Count is not (2 or 3))
            throw new ArgumentException("A plan requires two or three stages.");
        foreach (var stage in stages)
        {
            ChallengeEvaluator.Validate(
                new(
                    stage.ProfitTargetPercent,
                    stage.MaxDailyLossPercent,
                    stage.MaxTotalLossPercent,
                    stage.MinimumTradingDays,
                    stage.MaximumCalendarDays
                )
            );
            if (!await policies.IsActiveAsync(stage.PolicySetId, ct))
                throw new ArgumentException("Every stage must reference an active policy.");
        }
        version.Path = stages.Count == 2 ? "TwoStep" : "ThreeStep";
        version.Status = "Active";
        version.EffectiveFrom = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(ct);
        await audit.WriteAsync(
            "Subscription",
            "PlanVersion",
            versionId.ToString(),
            "Published",
            "Draft",
            "Active",
            ct
        );
    }
}
