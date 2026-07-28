namespace SubscriptionManager.Application.Common.Email;

public interface IEmailSender
{
    Task SendEmailConfirmationAsync(
        string email,
        Guid userId,
        string confirmationToken,
        CancellationToken cancellationToken = default);
}
