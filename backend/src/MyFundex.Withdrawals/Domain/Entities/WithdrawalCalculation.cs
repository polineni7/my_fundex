using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MyFundex.BuildingBlocks.Abstractions;
using MyFundex.BuildingBlocks.Domain;
using MyFundex.BuildingBlocks.Persistence;

namespace MyFundex.Withdrawals;

public sealed class WithdrawalCalculation : EntityBase
{
    public Guid CalculationId { get; set; }
    public long WithdrawalInternalId { get; set; }
    public decimal GrossProfit { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal PlatformFee { get; set; }
    public decimal PlatformFeeTax { get; set; }
    public decimal ProfitAfterDeductions { get; set; }
    public decimal TraderSharePercentage { get; set; }
    public decimal TraderEntitlement { get; set; }
    public decimal CompanyShare { get; set; }
    public Guid PolicyVersionId { get; set; }
}
