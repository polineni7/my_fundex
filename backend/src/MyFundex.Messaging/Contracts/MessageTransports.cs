namespace MyFundex.Messaging.Contracts;

public sealed record EmailMessage(string Recipient, string Subject, string Text);

public interface IEmailTransport
{
    Task SendAsync(EmailMessage message, CancellationToken cancellationToken);
}

public interface ISmsTransport
{
    Task SendAsync(string destination, string message, CancellationToken cancellationToken);
}
