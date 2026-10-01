using MyFundex.BuildingBlocks.Domain;

namespace MyFundex.Trading;

public sealed class ProfitDistribution : EntityBase
{
    public Guid DistributionId { get; set; }
    public Guid AccountId { get; set; }
    public long AccountInternalId { get; set; }
    public string SettlementReference { get; set; } = "";
    public decimal GrossProfit { get; set; }
    public decimal Fees { get; set; }
    public decimal TaxWithheld { get; set; }
    public decimal OtherDeductions { get; set; }
    public decimal TaxWithholdingPercent { get; set; }
    public decimal OtherDeductionPercent { get; set; }
    public decimal TraderReward { get; set; }
    public decimal PlatformReward { get; set; }
    public DateTimeOffset? DeliveredAt { get; set; }
}
