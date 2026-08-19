using Microsoft.Extensions.Logging;
using SubscriptionManager.Application.Authentication;

namespace SubscriptionManager.Infrastructure.Authentication.Email;

/// <summary>
/// Logs account emails instead of sending them during development.
/// </summary>
public sealed class DevelopmentEmailSender(
    AccountEmailLinkBuilder linkBuilder,
    ILogger<DevelopmentEmailSender> logger)
    : IEmailSender
{
    public Task SendEmailConfirmationAsync(
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

        LogDevelopmentEmail(
            email,
            content,
            link);

        return Task.CompletedTask;
    }

    public Task SendPasswordResetAsync(
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

        LogDevelopmentEmail(
            email,
            content,
            link);

        return Task.CompletedTask;
    }

    public Task SendPasswordChangedAsync(
        string email,
        string languageCode,
        CancellationToken cancellationToken = default)
    {
        var applicationBaseUrl =
            linkBuilder.BuildApplicationBaseUrl();

        var content =
            AccountEmailTemplates.PasswordChanged(
                languageCode,
                applicationBaseUrl);

        LogDevelopmentEmail(
            email,
            content);

        return Task.CompletedTask;
    }

    public Task SendAccountDeletedAsync(
        string email,
        string languageCode,
        CancellationToken cancellationToken = default)
    {
        var applicationBaseUrl =
            linkBuilder.BuildApplicationBaseUrl();

        var content =
            AccountEmailTemplates.AccountDeleted(
                languageCode,
                applicationBaseUrl);

        LogDevelopmentEmail(
            email,
            content);

        return Task.CompletedTask;
    }

    private void LogDevelopmentEmail(
        string email,
        AccountEmailContent content,
        string? actionLink = null)
    {
        if (string.IsNullOrWhiteSpace(actionLink))
        {
            logger.LogInformation(
                """
                Development email generated. No external message was sent.

                Recipient: {Recipient}
                Subject: {Subject}

                {TextBody}
                """,
                email,
                content.Subject,
                content.TextBody);

            return;
        }

        logger.LogInformation(
            """
            Development email generated. No external message was sent.

            Recipient: {Recipient}
            Subject: {Subject}
            Action link: {ActionLink}

            {TextBody}
            """,
            email,
            content.Subject,
            actionLink,
            content.TextBody);
    }
}
