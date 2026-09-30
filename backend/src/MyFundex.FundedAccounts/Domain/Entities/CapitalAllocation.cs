using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MyFundex.BuildingBlocks.Abstractions;
using MyFundex.BuildingBlocks.Domain;
using MyFundex.BuildingBlocks.Persistence;
using MyFundex.Contracts;

namespace MyFundex.FundedAccounts;

public sealed class CapitalAllocation : EntityBase, IImmutableRecord
{
    public Guid AllocationId { get; set; }
    public long FundedAccountInternalId { get; set; }
    public string AllocationType { get; set; } = "InitialFunding";
    public decimal Amount { get; set; }
    public DateTimeOffset EffectiveAt { get; set; }
    public string? ReferenceType { get; set; }
    public string? ReferenceId { get; set; }
}
