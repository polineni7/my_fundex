using MyFundex.BuildingBlocks.Domain;
namespace MyFundex.Payments;

public sealed class Coupon : EntityBase
{
    public Guid CouponId { get; set; } = Guid.NewGuid();
    public string Code { get; set; } = "";
    public string Name { get; set; } = "";
    public string DiscountType { get; set; } = "Percentage";
    public decimal Value { get; set; }
    public decimal MinimumFee { get; set; }
    public Guid? PlanId { get; set; }
    public Guid? UserId { get; set; }
    public DateTimeOffset StartsAt { get; set; }
    public DateTimeOffset EndsAt { get; set; }
    public int MaximumUses { get; set; } = 100;
    public int UsesPerUser { get; set; } = 1;
    public int ReservedUses { get; set; }
    public bool IsActive { get; set; }
}
