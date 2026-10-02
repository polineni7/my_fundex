using MyFundex.BuildingBlocks.Domain;
namespace MyFundex.Administration;
public sealed class Campaign : EntityBase
{
public Guid CampaignId { get; set; } = Guid.NewGuid();
public Guid AudienceId { get; set; }
public string Name { get; set; } = "";
public string Subject { get; set; } = "";
public string Body { get; set; } = "";
public string Status { get; set; } = "Draft";
public DateTimeOffset? ScheduledAt { get; set; }
public DateTimeOffset? QueuedAt { get; set; }
}
