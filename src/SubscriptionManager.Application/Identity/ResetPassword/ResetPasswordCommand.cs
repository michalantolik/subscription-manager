namespace SubscriptionManager.Application.Identity.ResetPassword;

public sealed record ResetPasswordCommand(
    Guid UserId,
    string ResetToken,
    string NewPassword);
