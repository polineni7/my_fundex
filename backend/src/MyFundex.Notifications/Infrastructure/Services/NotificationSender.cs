using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MyFundex.BuildingBlocks.Abstractions;
using MyFundex.BuildingBlocks.Domain;
using MyFundex.BuildingBlocks.Persistence;
using MyFundex.Contracts;

namespace MyFundex.Notifications;

public sealed class NotificationSender(NotificationDbContext db) : INotificationSender
{
    public async Task CreateAsync(
        long userId,
        string type,
        string title,
        string message,
        CancellationToken ct
    )
    {
        db.Add(
            new Notification
            {
                NotificationId = MyFundex.BuildingBlocks.Ids.Uuid7.NewGuid(),
                UserInternalId = userId,
                Type = type,
                Title = title,
                Message = message,
            }
        );
        await db.SaveChangesAsync(ct);
    }
}
