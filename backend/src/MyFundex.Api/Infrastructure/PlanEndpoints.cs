using Microsoft.EntityFrameworkCore;
using MyFundex.Contracts;
using MyFundex.Subscription;

namespace MyFundex.Api.Infrastructure;

public static class PlanEndpoints
{
    public static void MapPlanEndpoints(this WebApplication app)
    {
        var admin = app.MapGroup("/api/v1/admin/plans")
            .RequireAuthorization(p =>
                p.RequireAssertion(c =>
                    c.User.IsInRole("ADMIN")
                    || c.User.IsInRole("MANAGER")
                        && c.User.HasClaim(
                            "permission",
                            c.Resource is HttpContext context
                            && HttpMethods.IsGet(context.Request.Method)
                                ? "plans.read"
                                : "plans.write"
                        )
                )
            );
        admin.MapGet(
            "/",
            async (SubscriptionDbContext db, CancellationToken ct) =>
                Results.Ok(
                    await db
                        .Plans.AsNoTracking()
                        .OrderBy(x => x.Name)
                        .Select(x => new
                        {
                            x.PlanId,
                            x.Code,
                            x.Name,
                            x.Description,
                            x.Version,
                        })
                        .ToListAsync(ct)
                )
        );

        admin.MapGet(
            "/{planId:guid}/versions",
            async (Guid planId, SubscriptionDbContext db, CancellationToken ct) =>
                Results.Ok(
                    await (
                        from plan in db.Plans.AsNoTracking()
                        join version in db.PlanVersions.AsNoTracking()
                            on plan.Id equals version.PlanInternalId
                        where plan.PlanId == planId
                        orderby version.VersionNumber descending
                        select new
                        {
                            version.PlanVersionId,
                            version.VersionNumber,
                            version.Version,
                            version.Status,
                            version.Path,
                            version.RegistrationFee,
                            version.ChallengeCapital,
                            version.RewardSharePercent,
                            version.TaxWithholdingPercent,
                            version.OtherDeductionPercent,
                            version.FundedDailyLossPercent,
                            version.FundedTotalLossPercent,
                        }
                    ).ToListAsync(ct)
                )
        );
        admin.MapPut(
            "/{planId:guid}",
            async (
                Guid planId,
                EditPlanRequest request,
                SubscriptionDbContext db,
                IAuditWriter audit,
                CancellationToken ct
            ) =>
            {
                if (string.IsNullOrWhiteSpace(request.Name) || request.Name.Length > 150)
                    return Results.BadRequest();
                var plan = await db.Plans.SingleOrDefaultAsync(x => x.PlanId == planId, ct);
                if (plan == null)
                    return Results.NotFound();
                if (plan.Version != request.Version)
                    return Results.Conflict();
                var previous = plan.Name;
                plan.Name = request.Name.Trim();
                plan.Description = request.Description;
                await db.SaveChangesAsync(ct);
                await audit.WriteAsync(
                    "Subscription",
                    "Plan",
                    planId.ToString(),
                    "Updated",
                    previous,
                    plan.Name,
                    ct
                );
                return Results.Ok(new { plan.PlanId, plan.Version });
            }
        );
        admin.MapPost(
            "/versions/{versionId:guid}/publish",
            async (
                Guid versionId,
                PublishPlanRequest request,
                PlanPublicationService service,
                CancellationToken ct
            ) =>
            {
                await service.PublishAsync(versionId, request.Version, ct);
                return Results.NoContent();
            }
        );
        admin.MapPost(
            "/",
            async (
                CreatePlanRequest request,
                SubscriptionDbContext db,
                IAuditWriter audit,
                CancellationToken ct
            ) =>
            {
                if (
                    string.IsNullOrWhiteSpace(request.Code)
                    || string.IsNullOrWhiteSpace(request.Name)
                    || request.Code.Length > 50
                    || request.Name.Length > 150
                )
                    return Results.BadRequest(
                        new
                        {
                            message = "A code (up to 50 characters) and name (up to 150 characters) are required.",
                        }
                    );
                var code = request.Code.Trim().ToUpperInvariant();
                if (await db.Plans.AnyAsync(x => x.Code == code, ct))
                    return Results.Conflict(new { message = "Plan code already exists." });
                var plan = new Plan
                {
                    PlanId = Guid.NewGuid(),
                    Code = code,
                    Name = request.Name.Trim(),
                    Description = request.Description,
                };
                db.Add(plan);
                await db.SaveChangesAsync(ct);
                await audit.WriteAsync(
                    "Subscription",
                    "Plan",
                    plan.PlanId.ToString(),
                    "Created",
                    null,
                    plan.Name,
                    ct
                );
                return Results.Created($"/api/v1/plans/{plan.PlanId}", new { plan.PlanId });
            }
        );
        admin.MapPost(
            "/{planId:guid}/versions",
            async (
                Guid planId,
                CreatePlanVersionRequest request,
                SubscriptionDbContext db,
                IAuditWriter audit,
                CancellationToken ct
            ) =>
            {
                var plan = await db.Plans.SingleOrDefaultAsync(x => x.PlanId == planId, ct);
                if (plan == null)
                    return Results.NotFound();
                if (
                    request.ChallengeCapital <= 0
                    || request.RegistrationFee < 1
                    || decimal.Round(request.RegistrationFee, 2) != request.RegistrationFee
                    || request.Stages is null
                    || request.Stages.Length is not (2 or 3)
                    || request.Stages.Any(x =>
                        string.IsNullOrWhiteSpace(x.Name)
                        || x.StartingCapital <= 0
                        || x.PolicySetId == Guid.Empty
                    )
                )
                    return Results.BadRequest(
                        new
                        {
                            message = "Provide positive capital, a fee in whole paise, and at least one named stage with a policy.",
                        }
                    );
                await using var transaction = await db.Database.BeginTransactionAsync(ct);
                foreach (var stage in request.Stages)
                    ChallengeEvaluator.Validate(
                        new(
                            stage.ProfitTargetPercent,
                            stage.MaxDailyLossPercent,
                            stage.MaxTotalLossPercent,
                            stage.MinimumTradingDays,
                            stage.MaximumCalendarDays
                        )
                    );
                var next =
                    (
                        await db
                            .PlanVersions.Where(x => x.PlanInternalId == plan.Id)
                            .MaxAsync(x => (int?)x.VersionNumber, ct) ?? 0
                    ) + 1;
                if (
                    request.TaxWithholdingPercent < 0
                    || request.OtherDeductionPercent < 0
                    || request.TaxWithholdingPercent + request.OtherDeductionPercent >= 100
                    || request.RewardSharePercent is <= 0 or > 100
                    || request.FundedDailyLossPercent is <= 0 or > 100
                    || request.FundedTotalLossPercent is <= 0 or > 100
                )
                    return Results.BadRequest(
                        new { message = "Invalid funded risk or reward percentages." }
                    );
                var version = new PlanVersion
                {
                    PlanVersionId = Guid.NewGuid(),
                    PlanInternalId = plan.Id,
                    VersionNumber = next,
                    ChallengeCapital = request.ChallengeCapital,
                    RewardSharePercent = request.RewardSharePercent,
                    TaxWithholdingPercent = request.TaxWithholdingPercent,
                    OtherDeductionPercent = request.OtherDeductionPercent,
                    FundedDailyLossPercent = request.FundedDailyLossPercent,
                    FundedTotalLossPercent = request.FundedTotalLossPercent,
                    RegistrationFee = request.RegistrationFee,
                    Path = request.Stages.Length == 2 ? "TwoStep" : "ThreeStep",
                    Status = "Draft",
                    EffectiveFrom = DateTimeOffset.UtcNow,
                };
                db.Add(version);
                await db.SaveChangesAsync(ct);
                for (var index = 0; index < request.Stages.Length; index++)
                {
                    var stage = request.Stages[index];
                    db.Add(
                        new PlanStageDefinition
                        {
                            StageId = Guid.NewGuid(),
                            PlanVersionInternalId = version.Id,
                            StageNumber = index + 1,
                            Name = stage.Name.Trim(),
                            StartingCapital = stage.StartingCapital,
                            PolicySetId = stage.PolicySetId,
                            ProfitTargetPercent = stage.ProfitTargetPercent,
                            MaxDailyLossPercent = stage.MaxDailyLossPercent,
                            MaxTotalLossPercent = stage.MaxTotalLossPercent,
                            MinimumTradingDays = stage.MinimumTradingDays,
                            MaximumCalendarDays = stage.MaximumCalendarDays,
                        }
                    );
                }
                await db.SaveChangesAsync(ct);
                await transaction.CommitAsync(ct);
                await audit.WriteAsync(
                    "Subscription",
                    "PlanVersion",
                    version.PlanVersionId.ToString(),
                    "DraftCreated",
                    null,
                    null,
                    ct
                );
                return Results.Created(
                    $"/api/v1/plans/{planId}/versions",
                    new
                    {
                        version.PlanVersionId,
                        version.VersionNumber,
                        version.Status,
                    }
                );
            }
        );
    }
}

public sealed record CreatePlanRequest(string Code, string Name, string? Description);

public sealed record CreatePlanVersionRequest(
    decimal ChallengeCapital,
    decimal RegistrationFee,
    StageRequest[] Stages,
    decimal RewardSharePercent = 80m,
    decimal FundedDailyLossPercent = 5m,
    decimal FundedTotalLossPercent = 10m,
    decimal TaxWithholdingPercent = 0m,
    decimal OtherDeductionPercent = 0m
);

public sealed record StageRequest(
    string Name,
    decimal StartingCapital,
    Guid PolicySetId,
    decimal ProfitTargetPercent = 8m,
    decimal MaxDailyLossPercent = 5m,
    decimal MaxTotalLossPercent = 10m,
    int MinimumTradingDays = 5,
    int? MaximumCalendarDays = null
);

public sealed record EditPlanRequest(string Name, string? Description, long Version);

public sealed record PublishPlanRequest(long Version);
