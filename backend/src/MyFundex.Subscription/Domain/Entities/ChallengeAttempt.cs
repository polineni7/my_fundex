using MyFundex.BuildingBlocks.Domain;

namespace MyFundex.Subscription;

public sealed class ChallengeAttempt : EntityBase
{
    public Guid AttemptId { get; set; }
    public long SubscriptionInternalId { get; set; }
    public long StageInternalId { get; set; }
    public Guid? AccountId { get; set; }
    public string Status { get; set; } = "Provisioning";
    public DateTimeOffset StartedAt { get; set; }
    public DateTimeOffset? CompletedAt { get; set; }
}
