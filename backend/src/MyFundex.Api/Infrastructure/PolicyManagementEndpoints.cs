using Microsoft.EntityFrameworkCore;
using MyFundex.Contracts;
using MyFundex.Risk;

namespace MyFundex.Api.Infrastructure;

public static class PolicyManagementEndpoints
{
    public static void MapPolicyManagement(this WebApplication app)
    {
        var group = app.MapGroup("/api/v1/admin/policies")
            .RequireAuthorization(policy =>
                policy.RequireAssertion(context =>
                    context.User.IsInRole("ADMIN")
                    || context.User.IsInRole("MANAGER")
                        && context.User.HasClaim("permission", context.Resource is HttpContext http && HttpMethods.IsGet(http.Request.Method) ? "policies.read" : "policies.write")
                )
            );
        group.MapGet("/{policyId:guid}", async (Guid policyId, RiskDbContext db, CancellationToken ct) =>
        {
            var policy = await db.PolicySets.AsNoTracking().SingleOrDefaultAsync(p => p.PolicyId == policyId, ct);
            if (policy == null) return Results.NotFound();
            var versions = await db.PolicyVersions.AsNoTracking().Where(v => v.PolicySetInternalId == policy.Id)
                .OrderByDescending(v => v.VersionNumber).Select(v => new
                {
                    v.PolicyVersionId,
                    v.VersionNumber,
                    v.Status,
                    v.ApplicationMode,
                    rules = db.Rules.Where(r => r.PolicyVersionInternalId == v.Id).OrderBy(r => r.Priority)
                        .Select(r => new { r.RuleId, r.RuleCode, r.Name, r.IsEnabled, r.DecimalValue, r.StringValue, r.ViolationAction }).ToList()
                }).ToListAsync(ct);
            return Results.Ok(new { policy.PolicyId, policy.Name, policy.Code, versions });
        });
        group.MapPost(
            "/",
            async (
                CreatePolicyRequest request,
                RiskDbContext db,
                IAuditWriter audit,
                CancellationToken ct
            ) =>
            {
                if (
                    string.IsNullOrWhiteSpace(request.Name)
                    || request.Name.Length > 150
                    || string.IsNullOrWhiteSpace(request.Code)
                    || request.Code.Length > 50
                    || request.MaximumOrderValue <= 0
                )
                    return Results.BadRequest(
                        new { message = "Provide a name, code and positive maximum order value." }
                    );
                var window = TradingWindow.Encode(request.EntryStartTime, request.EntryEndTime);
                var code = request.Code.Trim().ToUpperInvariant();
                if (await db.PolicySets.AnyAsync(x => x.Code == code, ct))
                    return Results.Conflict(new { message = "A policy group with this code already exists." });
                await using var transaction = await db.Database.BeginTransactionAsync(ct);
                var policy = new PolicySet
                {
                    PolicyId = Guid.NewGuid(),
                    Code = code,
                    Name = request.Name.Trim(),
                };
                db.Add(policy);
                await db.SaveChangesAsync(ct);
                var version = new PolicyVersion
                {
                    PolicyVersionId = Guid.NewGuid(),
                    PolicySetInternalId = policy.Id,
                    VersionNumber = 1,
                    Status = "Active",
                    ApplicationMode = "NewAccountsOnly",
                    EffectiveFrom = DateTimeOffset.UtcNow,
                };
                db.Add(version);
                await db.SaveChangesAsync(ct);
                db.AddRange(
                    new PolicyRule
                    {
                        RuleId = Guid.NewGuid(),
                        PolicyVersionInternalId = version.Id,
                        RuleCode = "EQUITY_ONLY",
                        Name = "Equity instruments only",
                        Priority = 1,
                    },
                    new PolicyRule
                    {
                        RuleId = Guid.NewGuid(),
                        PolicyVersionInternalId = version.Id,
                        RuleCode = "MAX_ORDER_VALUE",
                        Name = "Maximum order value",
                        DecimalValue = request.MaximumOrderValue,
                        Priority = 2,
                    }
                );
                if (window != null)
                    db.Add(new PolicyRule
                    {
                        RuleId = Guid.NewGuid(),
                        PolicyVersionInternalId = version.Id,
                        RuleCode = "ENTRY_TIME_WINDOW",
                        Name = "Order entry window (India time)",
                        StringValue = window,
                        Priority = 3
                    });
                await db.SaveChangesAsync(ct);
                await transaction.CommitAsync(ct);
                await audit.WriteAsync(
                    "Risk",
                    "PolicySet",
                    policy.PolicyId.ToString(),
                    "Created",
                    null,
                    policy.Name,
                    ct
                );
                return Results.Created(
                    $"/api/v1/admin/policies/{policy.PolicyId}",
                    new { policy.PolicyId, version.PolicyVersionId }
                );
            }
        );
    }
}

public sealed record CreatePolicyRequest(string Code, string Name, decimal MaximumOrderValue, string? EntryStartTime = null, string? EntryEndTime = null);
