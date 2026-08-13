namespace SubscriptionManager.Application.Authentication.LoginUser;

/// <summary>
/// Request to authenticate a user.
/// </summary>
public sealed record LoginUserCommand(
    string Email,
    string Password);
