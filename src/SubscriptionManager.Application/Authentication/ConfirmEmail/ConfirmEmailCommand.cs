namespace SubscriptionManager.Application.Authentication.ConfirmEmail;

/// <summary>
/// Request to confirm a user's email address.
/// </summary>
public sealed record ConfirmEmailCommand(
    Guid UserId,
    string ConfirmationToken);
