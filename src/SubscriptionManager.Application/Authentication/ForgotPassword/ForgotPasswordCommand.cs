namespace SubscriptionManager.Application.Authentication.ForgotPassword;

/// <summary>
/// Request to initiate a password reset.
/// </summary>
public sealed record ForgotPasswordCommand(
    string Email,
    string LanguageCode);
