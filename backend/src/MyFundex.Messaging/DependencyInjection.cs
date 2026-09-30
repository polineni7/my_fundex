using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MyFundex.Messaging.Contracts;
using MyFundex.Messaging.Infrastructure.Services;

namespace MyFundex.Messaging;

public static class DependencyInjection
{
    public static IServiceCollection AddMessaging(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        services.Configure<SmtpOptions>(configuration.GetSection("Messaging:Smtp"));
        services.AddTransient<IEmailTransport, SmtpEmailTransport>();
        services.AddTransient<ISmsTransport, DisabledSmsTransport>();
        return services;
    }
}
