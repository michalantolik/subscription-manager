namespace SubscriptionManager.Application.Common.Email;

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
