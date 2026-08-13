namespace SubscriptionManager.Application.Authentication.ResetPassword;

/// <summary>
/// Request to reset a user's password.
/// </summary>
public sealed record ResetPasswordCommand(
    Guid UserId,
    string ResetToken,
    string NewPassword);
