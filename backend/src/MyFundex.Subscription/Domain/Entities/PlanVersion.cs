using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MyFundex.BuildingBlocks.Abstractions;
using MyFundex.BuildingBlocks.Domain;
using MyFundex.BuildingBlocks.Persistence;

namespace MyFundex.Subscription;

public sealed class PlanVersion : EntityBase
{
    public decimal TaxWithholdingPercent { get; set; }
    public decimal OtherDeductionPercent { get; set; }
    public decimal FundedDailyLossPercent { get; set; } = 5m;
    public decimal FundedTotalLossPercent { get; set; } = 10m;
    public string Path { get; set; } = "TwoStep";
    public decimal RewardSharePercent { get; set; } = 80m;
    public Guid PlanVersionId { get; set; }
    public long PlanInternalId { get; set; }
    public int VersionNumber { get; set; }
    public decimal ChallengeCapital { get; set; }
    public decimal RegistrationFee { get; set; }
    public string Status { get; set; } = "Draft";
    public DateTimeOffset EffectiveFrom { get; set; }
    public DateTimeOffset? EffectiveTo { get; set; }
}
