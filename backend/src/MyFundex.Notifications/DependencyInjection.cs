using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MyFundex.BuildingBlocks.Abstractions;
using MyFundex.BuildingBlocks.Domain;
using MyFundex.BuildingBlocks.Persistence;
using MyFundex.Contracts;

namespace MyFundex.Notifications;

public static class NotificationsModule
{
    public static IServiceCollection AddNotificationsModule(this IServiceCollection s, string cs)
    {
        s.AddDbContext<NotificationDbContext>(o => o.UseNpgsql(cs));
        s.AddScoped<INotificationSender, NotificationSender>();
        return s;
    }
}
