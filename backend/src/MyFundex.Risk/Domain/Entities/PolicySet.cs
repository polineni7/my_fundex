using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MyFundex.BuildingBlocks.Abstractions;
using MyFundex.BuildingBlocks.Domain;
using MyFundex.BuildingBlocks.Persistence;
using MyFundex.Contracts;

namespace MyFundex.Risk;

public sealed class PolicySet : EntityBase
{
    public Guid PolicyId { get; set; }
    public string Code { get; set; } = "";
    public string Name { get; set; } = "";
    public string PolicyType { get; set; } = "Trading";
}
