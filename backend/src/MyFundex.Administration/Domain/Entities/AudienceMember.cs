using MyFundex.BuildingBlocks.Domain;
namespace MyFundex.Administration;
public sealed class AudienceMember : EntityBase
{
public Guid MemberId { get; set; } = Guid.NewGuid();
public Guid AudienceId { get; set; }
public string Email { get; set; } = "";
public string Name { get; set; } = "";
public bool MarketingConsent { get; set; }
public string ConsentSource { get; set; } = "";
public DateTimeOffset? ConsentedAt { get; set; }
public DateTimeOffset? UnsubscribedAt { get; set; }
public Guid UnsubscribeToken { get; set; } = Guid.NewGuid();
}
