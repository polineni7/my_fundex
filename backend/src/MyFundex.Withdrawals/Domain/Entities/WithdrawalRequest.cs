using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MyFundex.BuildingBlocks.Abstractions;
using MyFundex.BuildingBlocks.Domain;
using MyFundex.BuildingBlocks.Persistence;

namespace MyFundex.Withdrawals;

public sealed class WithdrawalRequest : EntityBase
{
    public Guid WithdrawalId { get; set; }
    public long FundedAccountInternalId { get; set; }
    public decimal RequestedAmount { get; set; }
    public decimal EligibleAmount { get; set; }
    public decimal NetPayoutAmount { get; set; }
    public string Status { get; set; } = "Requested";
    public DateTimeOffset RequestedAt { get; set; } = DateTimeOffset.UtcNow;
}
