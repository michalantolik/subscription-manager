namespace SubscriptionManager.Application.Identity.LoginUser;

public sealed record LoginUserCommand(
    string Email,
    string Password);
