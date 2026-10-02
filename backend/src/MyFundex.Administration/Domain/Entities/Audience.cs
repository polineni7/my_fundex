using MyFundex.BuildingBlocks.Domain;
namespace MyFundex.Administration;
public sealed class Audience : EntityBase
{
public Guid AudienceId { get; set; } = Guid.NewGuid();
public string Name { get; set; } = "";
public string Description { get; set; } = "";
}
