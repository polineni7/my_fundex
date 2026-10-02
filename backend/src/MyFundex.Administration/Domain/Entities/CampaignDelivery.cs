using MyFundex.BuildingBlocks.Domain;
namespace MyFundex.Administration;
public sealed class CampaignDelivery : EntityBase
{
public Guid DeliveryId { get; set; } = Guid.NewGuid();
public Guid CampaignId { get; set; }
public Guid MemberId { get; set; }
public string Email { get; set; } = "";
public string Status { get; set; } = "Pending";
public int Attempts { get; set; }
public DateTimeOffset? LeaseUntil { get; set; }
public DateTimeOffset? SentAt { get; set; }
public string? Error { get; set; }
}
