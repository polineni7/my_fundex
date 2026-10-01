using MyFundex.BuildingBlocks.Domain;

namespace MyFundex.Wallet;

public sealed class WithdrawalHold : EntityBase
{
    public Guid WithdrawalId { get; set; }
    public long WalletInternalId { get; set; }
    public decimal Amount { get; set; }
    public string Status { get; set; } = "Reserved";
}
