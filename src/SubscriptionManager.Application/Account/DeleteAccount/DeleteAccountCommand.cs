namespace SubscriptionManager.Application.Account.DeleteAccount;

/// <summary>
/// Request to delete a user account.
/// </summary>
public sealed record DeleteAccountCommand(
    Guid UserId);
