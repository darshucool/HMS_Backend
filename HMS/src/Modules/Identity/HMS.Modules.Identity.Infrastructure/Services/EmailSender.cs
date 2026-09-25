using System.Net;
using System.Net.Mail;
using HMS.Modules.Identity.Application.Abstractions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace HMS.Modules.Identity.Infrastructure.Services;

internal sealed class EmailSender(
    IConfiguration configuration,
    ILogger<EmailSender> logger) : IEmailSender
{
    public async Task<EmailSendResult> SendAsync(
        string to,
        string subject,
        string body,
        CancellationToken cancellationToken)
    {
        var from = configuration["Email:From"] ?? "noreply@hms.local";
        var host = configuration["Email:Smtp:Host"];

        if (string.IsNullOrWhiteSpace(host))
        {
            logger.LogWarning(
                "Email not sent via SMTP (Email:Smtp:Host is empty). To={To} Subject={Subject}\n{Body}",
                to,
                subject,
                body);

            return new EmailSendResult(
                false,
                "SMTP is not configured. Set Email:Smtp:Host (and credentials) in appsettings.");
        }

        var port = int.TryParse(configuration["Email:Smtp:Port"], out var parsedPort) ? parsedPort : 587;
        var username = configuration["Email:Smtp:Username"];
        var password = configuration["Email:Smtp:Password"];
        var enableSsl = !string.Equals(
            configuration["Email:Smtp:EnableSsl"],
            "false",
            StringComparison.OrdinalIgnoreCase);

        try
        {
            using var client = new SmtpClient(host, port)
            {
                EnableSsl = enableSsl,
                DeliveryMethod = SmtpDeliveryMethod.Network
            };

            if (!string.IsNullOrWhiteSpace(username))
                client.Credentials = new NetworkCredential(username, password);

            using var message = new MailMessage(from, to, subject, body);
            await client.SendMailAsync(message, cancellationToken);

            logger.LogInformation("Invite email sent to {To}. Subject={Subject}", to, subject);
            return new EmailSendResult(true, "Email sent.");
        }
        catch (Exception exception)
        {
            logger.LogError(
                exception,
                "Failed to send email to {To}. Subject={Subject}\n{Body}",
                to,
                subject,
                body);

            return new EmailSendResult(false, $"SMTP send failed: {exception.Message}");
        }
    }
}
