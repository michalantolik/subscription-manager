namespace SubscriptionManager.Application.Authentication;

/// <summary>
/// Sends authentication-related emails to application users.
/// </summary>
public interface IEmailSender
{
    Task SendEmailConfirmationAsync(
        string email,
        Guid userId,
        string confirmationToken,
        string languageCode,
        CancellationToken cancellationToken = default);

    Task SendPasswordResetAsync(
        string email,
        Guid userId,
        string resetToken,
        string languageCode,
        CancellationToken cancellationToken = default);
}
