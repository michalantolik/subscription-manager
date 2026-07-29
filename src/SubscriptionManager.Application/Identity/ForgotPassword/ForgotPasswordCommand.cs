namespace SubscriptionManager.Application.Identity.ForgotPassword;

public sealed record ForgotPasswordCommand(
    string Email,
    string LanguageCode);
