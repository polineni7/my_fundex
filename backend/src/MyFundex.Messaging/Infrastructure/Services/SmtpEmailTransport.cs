using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Options;
using MyFundex.Messaging.Contracts;

namespace MyFundex.Messaging.Infrastructure.Services;

public sealed class SmtpOptions
{
    public string Host { get; set; } = "";
    public int Port { get; set; } = 587;
    public string Sender { get; set; } = "";
    public string Username { get; set; } = "";
    public string Password { get; set; } = "";
}

public sealed class SmtpEmailTransport(IOptions<SmtpOptions> options) : IEmailTransport
{
    public async Task SendAsync(EmailMessage message, CancellationToken cancellationToken)
    {
        var settings = options.Value;
        if (string.IsNullOrWhiteSpace(settings.Host) || string.IsNullOrWhiteSpace(settings.Sender))
            throw new InvalidOperationException("SMTP is not configured.");
        using var client = new SmtpClient(settings.Host, settings.Port)
        {
            EnableSsl = true,
            UseDefaultCredentials = false,
            Credentials = new NetworkCredential(settings.Username, settings.Password),
        };
        using var mail = new MailMessage(
            settings.Sender,
            message.Recipient,
            message.Subject,
            message.Text
        );
        await client.SendMailAsync(mail, cancellationToken);
    }
}

public sealed class DisabledSmsTransport : ISmsTransport
{
    public Task SendAsync(
        string destination,
        string message,
        CancellationToken cancellationToken
    ) =>
        Task.FromException(
            new InvalidOperationException(
                "SMS delivery is disabled until a provider is configured."
            )
        );
}
