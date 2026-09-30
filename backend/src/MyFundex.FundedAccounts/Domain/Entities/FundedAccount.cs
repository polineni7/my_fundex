using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MyFundex.BuildingBlocks.Abstractions;
using MyFundex.BuildingBlocks.Domain;
using MyFundex.BuildingBlocks.Persistence;
using MyFundex.Contracts;

namespace MyFundex.FundedAccounts;

public sealed class FundedAccount : EntityBase
{
    public Guid? ProvisioningId { get; set; }
    public string TradingMode { get; set; } = "Evaluation";
    public string BrokerProvider { get; set; } = "Upstox";
    public string? BrokerCredentialKey { get; set; }
    public Guid AccountId { get; set; }
    public long UserInternalId { get; set; }
    public long SubscriptionInternalId { get; set; }
    public string AccountNumber { get; set; } = "";
    public string Status { get; set; } = "Active";
    public decimal FundedCapital { get; set; }
    public decimal WalletContribution { get; set; }
    public decimal CurrentBuyingPower { get; set; }
    public string CurrencyCode { get; set; } = "INR";
    public Guid? ActivePolicyAssignmentId { get; set; }
    public DateTimeOffset? ActivatedAt { get; set; }
    public DateTimeOffset? SuspendedAt { get; set; }
    public DateTimeOffset? ClosedAt { get; set; }
}
