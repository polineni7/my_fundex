using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MyFundex.BuildingBlocks.Abstractions;
using MyFundex.BuildingBlocks.Domain;
using MyFundex.BuildingBlocks.Persistence;
using MyFundex.Contracts;

namespace MyFundex.Wallet;

public sealed class WalletAccount : EntityBase
{
    public Guid WalletId { get; set; }
    public long FundedAccountInternalId { get; set; }
    public string CurrencyCode { get; set; } = "INR";
    public string Status { get; set; } = "Active";
    public decimal CachedAvailableBalance { get; set; }
    public decimal CachedWithdrawableBalance { get; set; }
}
