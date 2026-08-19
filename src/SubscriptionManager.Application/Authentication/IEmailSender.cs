namespace SubscriptionManager.Application.Authentication;

/// <summary>
/// Sends account-related emails to application users.
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

    Task SendPasswordChangedAsync(
        string email,
        string languageCode,
        CancellationToken cancellationToken = default);

    Task SendAccountDeletedAsync(
        string email,
        string languageCode,
        CancellationToken cancellationToken = default);
}
