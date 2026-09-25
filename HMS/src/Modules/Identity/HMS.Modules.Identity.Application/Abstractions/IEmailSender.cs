namespace HMS.Modules.Identity.Application.Abstractions;

public sealed record EmailSendResult(bool Sent, string Detail);

public interface IEmailSender
{
    Task<EmailSendResult> SendAsync(
        string to,
        string subject,
        string body,
        CancellationToken cancellationToken);
}
