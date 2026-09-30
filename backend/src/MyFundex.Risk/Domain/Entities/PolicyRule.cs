using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MyFundex.BuildingBlocks.Abstractions;
using MyFundex.BuildingBlocks.Domain;
using MyFundex.BuildingBlocks.Persistence;
using MyFundex.Contracts;

namespace MyFundex.Risk;

public sealed class PolicyRule : EntityBase
{
    public Guid RuleId { get; set; }
    public long PolicyVersionInternalId { get; set; }
    public string RuleCode { get; set; } = "";
    public string Name { get; set; } = "";
    public string Category { get; set; } = "Trading";
    public string Operator { get; set; } = "GreaterThan";
    public string ViolationAction { get; set; } = "RejectOrder";
    public int Priority { get; set; }
    public bool IsEnabled { get; set; } = true;
    public decimal? DecimalValue { get; set; }
    public int? IntegerValue { get; set; }
    public string? StringValue { get; set; }
}
