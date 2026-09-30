using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MyFundex.BuildingBlocks.Abstractions;
using MyFundex.BuildingBlocks.Domain;
using MyFundex.BuildingBlocks.Persistence;

namespace MyFundex.Subscription;

public sealed class PlanVersion : EntityBase
{
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
