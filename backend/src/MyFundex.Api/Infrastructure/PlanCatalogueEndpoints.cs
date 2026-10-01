using Microsoft.EntityFrameworkCore;
using MyFundex.Subscription;

namespace MyFundex.Api.Infrastructure;

public static class PlanCatalogueEndpoints
{
    public static void MapPlanCatalogue(this WebApplication app)
    {
        app.MapGet(
                "/api/v1/plan-catalogue",
                async (SubscriptionDbContext db, CancellationToken ct) =>
                {
                    var now = DateTimeOffset.UtcNow;
                    var versions = await (
                        from plan in db.Plans.AsNoTracking()
                        join version in db.PlanVersions.AsNoTracking()
                            on plan.Id equals version.PlanInternalId
                        where
                            version.Status == "Active"
                            && version.EffectiveFrom <= now
                            && (version.EffectiveTo == null || version.EffectiveTo > now)
                        orderby version.ChallengeCapital, version.VersionNumber descending
                        select new
                        {
                            InternalId = version.Id,
                            plan.PlanId,
                            plan.Name,
                            plan.Description,
                            version.PlanVersionId,
                            version.Path,
                            version.ChallengeCapital,
                            version.RegistrationFee,
                            version.RewardSharePercent,
                            version.TaxWithholdingPercent,
                            version.OtherDeductionPercent,
                            version.FundedDailyLossPercent,
                            version.FundedTotalLossPercent,
                        }
                    ).Take(100).ToListAsync(ct);
                    var ids = versions.Select(x => x.InternalId).ToArray();
                    var stages = await db
                        .Stages.AsNoTracking()
                        .Where(x => ids.Contains(x.PlanVersionInternalId))
                        .OrderBy(x => x.StageNumber)
                        .Select(x => new
                        {
                            InternalId = x.PlanVersionInternalId,
                            x.StageNumber,
                            x.Name,
                            x.ProfitTargetPercent,
                            x.MaxDailyLossPercent,
                            x.MaxTotalLossPercent,
                            x.MinimumTradingDays,
                            x.MaximumCalendarDays,
                            x.TradingPeriod,
                            x.MaximumLeverage,
                        })
                        .ToListAsync(ct);
                    var lookup = stages.ToLookup(x => x.InternalId);
                    return Results.Ok(
                        versions.Select(x => new
                        {
                            x.PlanId,
                            x.Name,
                            x.Description,
                            x.PlanVersionId,
                            x.Path,
                            x.ChallengeCapital,
                            x.RegistrationFee,
                            x.RewardSharePercent,
                            x.TaxWithholdingPercent,
                            x.OtherDeductionPercent,
                            x.FundedDailyLossPercent,
                            x.FundedTotalLossPercent,
                            currency = "INR",
                            stages = lookup[x.InternalId]
                                .Select(stage => new
                                {
                                    stage.StageNumber,
                                    stage.Name,
                                    stage.ProfitTargetPercent,
                                    stage.MaxDailyLossPercent,
                                    stage.MaxTotalLossPercent,
                                    stage.MinimumTradingDays,
                                    stage.MaximumCalendarDays,
                                    stage.TradingPeriod,
                                    stage.MaximumLeverage,
                                    tradingPeriodLabel = TradingPeriods.Label(stage.TradingPeriod, stage.MaximumCalendarDays),
                                }),
                        })
                    );
                }
            )
            .AllowAnonymous();
    }
}
