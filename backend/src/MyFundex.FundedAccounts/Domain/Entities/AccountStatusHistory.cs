using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MyFundex.BuildingBlocks.Abstractions;
using MyFundex.BuildingBlocks.Domain;
using MyFundex.BuildingBlocks.Persistence;
using MyFundex.Contracts;

namespace MyFundex.FundedAccounts;

public sealed class AccountStatusHistory : EntityBase, IImmutableRecord
{
    public Guid StatusHistoryId { get; set; }
    public long FundedAccountInternalId { get; set; }
    public string? FromStatus { get; set; }
    public string ToStatus { get; set; } = "";
    public string? ReasonCode { get; set; }
    public string? Reason { get; set; }
    public DateTimeOffset ChangedAt { get; set; }
}
