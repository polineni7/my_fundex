using Microsoft.EntityFrameworkCore;
using MyFundex.Contracts;
namespace MyFundex.Risk;

public sealed class RiskGuard(RiskDbContext db):IRiskGuard
{
    public async Task<RiskCheckResult> EvaluateAsync(RiskCheckRequest request,CancellationToken ct)
    {
        var now=DateTimeOffset.UtcNow;
        var rules=await (from assignment in db.Assignments.AsNoTracking()
            join version in db.PolicyVersions.AsNoTracking() on assignment.PolicyVersionInternalId equals version.Id
            join rule in db.Rules.AsNoTracking() on version.Id equals rule.PolicyVersionInternalId
            where assignment.FundedAccountInternalId==request.AccountInternalId && rule.IsEnabled && version.Status=="Active"
                && assignment.EffectiveFrom<=now && (assignment.EffectiveTo==null || assignment.EffectiveTo>now)
                && version.EffectiveFrom<=now && (version.EffectiveTo==null || version.EffectiveTo>now)
            orderby rule.Priority select rule).ToListAsync(ct);
        if(rules.Count==0) return new(false,"POLICY_MISSING","No effective trading policy is assigned.");
        if(!request.InstrumentToken.StartsWith("NSE_EQ|",StringComparison.Ordinal) && !request.InstrumentToken.StartsWith("BSE_EQ|",StringComparison.Ordinal))
            return new(false,"EQUITY_ONLY","Only equity instruments are supported.");
        foreach(var rule in rules)
        {
            if(rule.RuleCode is not ("MAX_ORDER_VALUE" or "EQUITY_ONLY"))
                return new(false,"UNSUPPORTED_POLICY",$"Rule {rule.RuleCode} is not implemented; trading is blocked until it can be evaluated.");
            if(rule.RuleCode=="MAX_ORDER_VALUE")
            {
                if(rule.DecimalValue is null or <= 0) return new(false,"INVALID_POLICY","Order value limit is invalid.");
                if(request.EstimatedValue>rule.DecimalValue)
                {
                    db.Add(new PolicyViolation { ViolationId=Guid.NewGuid(),FundedAccountInternalId=request.AccountInternalId,
                        PolicyRuleInternalId=rule.Id,ObservedValue=request.EstimatedValue.ToString(System.Globalization.CultureInfo.InvariantCulture),
                        LimitValue=rule.DecimalValue.Value.ToString(System.Globalization.CultureInfo.InvariantCulture),ActionTaken="RejectOrder",OccurredAt=now });
                    await db.SaveChangesAsync(ct);
                    return new(false,rule.RuleCode,"Order value exceeds the assigned limit.");
                }
            }
        }
        return new(true);
    }
}
