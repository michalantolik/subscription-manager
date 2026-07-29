using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SubscriptionManager.Application.Common.Email;

namespace SubscriptionManager.Infrastructure.Email;

public sealed class DevelopmentEmailSender(
    IOptions<EmailOptions> options,
    ILogger<DevelopmentEmailSender> logger)
    : IEmailSender
{
    private readonly EmailOptions _options = options.Value;

    public Task SendEmailConfirmationAsync(
        string email,
        Guid userId,
        string confirmationToken,
        string languageCode,
        CancellationToken cancellationToken = default)
    {
        var link = BuildLink(
            "/confirm-email",
            userId,
            "token",
            confirmationToken);

        var content = AccountEmailTemplates.EmailConfirmation(
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
        var link = BuildLink(
            "/reset-password",
            userId,
            "token",
            resetToken);

        var content = AccountEmailTemplates.PasswordReset(
            languageCode,
            link);

        LogDevelopmentEmail(
            email,
            content,
            link);

        return Task.CompletedTask;
    }

    private string BuildLink(
        string path,
        Guid userId,
        string tokenParameterName,
        string token)
    {
        var baseUrl = _options.ApplicationBaseUrl.TrimEnd('/');
        var url = $"{baseUrl}{path}";

        return QueryHelpers.AddQueryString(
            url,
            new Dictionary<string, string?>
            {
                ["userId"] = userId.ToString(),
                [tokenParameterName] = token
            });
    }

    private void LogDevelopmentEmail(
        string email,
        AccountEmailContent content,
        string actionLink)
    {
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
