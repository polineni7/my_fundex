using MyFundex.BuildingBlocks.Domain;

namespace MyFundex.Identity;

public sealed class IdentityEvent : EntityBase, IImmutableRecord
{
    public Guid EventId { get; set; } = Guid.NewGuid();
    public long? UserInternalId { get; set; }
    public string EventType { get; set; } = "";
    public string Method { get; set; } = "";
    public bool Succeeded { get; set; }
    public string? Detail { get; set; }
}
