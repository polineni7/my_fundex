using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MyFundex.BuildingBlocks.Abstractions;
using MyFundex.BuildingBlocks.Domain;
using MyFundex.BuildingBlocks.Persistence;
using MyFundex.Contracts;

namespace MyFundex.Notifications;

public sealed class Notification : EntityBase
{
    public Guid NotificationId { get; set; }
    public long UserInternalId { get; set; }
    public string Type { get; set; } = "General";
    public string Title { get; set; } = "";
    public string Message { get; set; } = "";
    public bool IsRead { get; set; }
    public DateTimeOffset? ReadAt { get; set; }
}
