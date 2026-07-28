using Microsoft.Extensions.Logging;
using SubscriptionManager.Application.Common.Email;

namespace SubscriptionManager.Infrastructure.Email;

public sealed class DevelopmentEmailSender(
    ILogger<DevelopmentEmailSender> logger)
    : IEmailSender
{
    public Task SendEmailConfirmationAsync(
        string email,
        Guid userId,
        string confirmationToken,
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation(
            """
            Email confirmation

            Email: {Email}
            UserId: {UserId}
            Token: {Token}
            """,
            email,
            userId,
            confirmationToken);

        return Task.CompletedTask;
    }
}
