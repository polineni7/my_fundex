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
                        && context.User.HasClaim("permission", "policies.write")
                )
            );
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
                var code = request.Code.Trim().ToUpperInvariant();
                if (await db.PolicySets.AnyAsync(x => x.Code == code, ct))
                    return Results.Conflict();
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

public sealed record CreatePolicyRequest(string Code, string Name, decimal MaximumOrderValue);
