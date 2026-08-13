using Azure.Communication.Email;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SubscriptionManager.Application.Authentication;

namespace SubscriptionManager.Infrastructure.Email;

public sealed class AzureEmailSender(
    EmailClient emailClient,
    AccountEmailLinkBuilder linkBuilder,
    IOptions<AzureEmailOptions> options,
    ILogger<AzureEmailSender> logger)
    : IEmailSender
{
    private readonly AzureEmailOptions _options = options.Value;

    public async Task SendEmailConfirmationAsync(
        string email,
        Guid userId,
        string confirmationToken,
        string languageCode,
        CancellationToken cancellationToken = default)
    {
        var link =
            linkBuilder.BuildEmailConfirmationLink(
                userId,
                confirmationToken);

        var content =
            AccountEmailTemplates.EmailConfirmation(
                languageCode,
                link);

        await SendAsync(
            email,
            content,
            cancellationToken);
    }

    public async Task SendPasswordResetAsync(
        string email,
        Guid userId,
        string resetToken,
        string languageCode,
        CancellationToken cancellationToken = default)
    {
        var link =
            linkBuilder.BuildPasswordResetLink(
                userId,
                resetToken);

        var content =
            AccountEmailTemplates.PasswordReset(
                languageCode,
                link);

        await SendAsync(
            email,
            content,
            cancellationToken);
    }

    private async Task SendAsync(
        string recipient,
        AccountEmailContent content,
        CancellationToken cancellationToken)
    {
        var message =
            new EmailMessage(
                _options.SenderAddress,
                recipient,
                new EmailContent(content.Subject)
                {
                    PlainText = content.TextBody
                });

        try
        {
            await emailClient.SendAsync(
                Azure.WaitUntil.Completed,
                message,
                cancellationToken);

            logger.LogInformation(
                "Account email sent successfully to {Recipient}.",
                recipient);
        }
        catch (Exception exception)
            when (exception is not OperationCanceledException)
        {
            logger.LogError(
                exception,
                "Failed to send account email to {Recipient}.",
                recipient);

            throw;
        }
    }
}
