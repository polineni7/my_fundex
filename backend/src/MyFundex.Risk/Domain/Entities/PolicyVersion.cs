using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MyFundex.BuildingBlocks.Abstractions;
using MyFundex.BuildingBlocks.Domain;
using MyFundex.BuildingBlocks.Persistence;
using MyFundex.Contracts;

namespace MyFundex.Risk;

public sealed class PolicyVersion : EntityBase
{
    public Guid PolicyVersionId { get; set; }
    public long PolicySetInternalId { get; set; }
    public int VersionNumber { get; set; }
    public string Status { get; set; } = "Draft";
    public DateTimeOffset EffectiveFrom { get; set; }
    public DateTimeOffset? EffectiveTo { get; set; }
    public string ApplicationMode { get; set; } = "NextTrade";
}
