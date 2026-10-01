using MyFundex.BuildingBlocks.Domain;

namespace MyFundex.Subscription;

public sealed class ChallengeNotice : EntityBase
{
    public Guid NoticeId { get; set; } = Guid.NewGuid();
    public long UserInternalId { get; set; }
    public long SubscriptionInternalId { get; set; }
    public string Outcome { get; set; } = "";
    public DateTimeOffset? DeliveredAt { get; set; }
    public DateTimeOffset? LeaseUntil { get; set; }
    public int Attempts { get; set; }
}
