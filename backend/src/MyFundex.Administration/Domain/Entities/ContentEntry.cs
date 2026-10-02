using MyFundex.BuildingBlocks.Domain;
namespace MyFundex.Administration;
public sealed class ContentEntry : EntityBase
{
public Guid ContentId { get; set; } = Guid.NewGuid();
public string Kind { get; set; } = "Page";
public string Slug { get; set; } = "";
public string Title { get; set; } = "";
public string Body { get; set; } = "";
public string Status { get; set; } = "Draft";
public DateTimeOffset? StartsAt { get; set; }
public DateTimeOffset? EndsAt { get; set; }
}
