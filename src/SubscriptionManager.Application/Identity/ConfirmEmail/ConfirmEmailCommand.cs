namespace SubscriptionManager.Application.Identity.ConfirmEmail;

public sealed record ConfirmEmailCommand(
    Guid UserId,
    string ConfirmationToken);
