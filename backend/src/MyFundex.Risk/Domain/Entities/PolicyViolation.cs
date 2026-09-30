using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MyFundex.BuildingBlocks.Abstractions;
using MyFundex.BuildingBlocks.Domain;
using MyFundex.BuildingBlocks.Persistence;
using MyFundex.Contracts;

namespace MyFundex.Risk;

public sealed class PolicyViolation : EntityBase
{
    public Guid ViolationId { get; set; }
    public long FundedAccountInternalId { get; set; }
    public long PolicyRuleInternalId { get; set; }
    public string? ObservedValue { get; set; }
    public string? LimitValue { get; set; }
    public string ActionTaken { get; set; } = "";
    public DateTimeOffset OccurredAt { get; set; }
}
