using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MyFundex.BuildingBlocks.Abstractions;
using MyFundex.BuildingBlocks.Domain;
using MyFundex.BuildingBlocks.Persistence;

namespace MyFundex.Subscription;

public sealed class PlanStageDefinition : EntityBase
{
    public decimal ProfitTargetPercent { get; set; } = 8m;
    public decimal MaxDailyLossPercent { get; set; } = 5m;
    public decimal MaxTotalLossPercent { get; set; } = 10m;
    public int MinimumTradingDays { get; set; } = 5;
    public int? MaximumCalendarDays { get; set; }
    public Guid StageId { get; set; }
    public long PlanVersionInternalId { get; set; }
    public int StageNumber { get; set; }
    public string Name { get; set; } = "";
    public decimal StartingCapital { get; set; }
    public Guid PolicySetId { get; set; }
    public bool RequiredForCompletion { get; set; } = true;
}
