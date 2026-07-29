namespace SubscriptionManager.Application.Identity.RegisterUser;

public sealed record RegisterUserCommand(
    string Email,
    string Password,
    string LanguageCode);
