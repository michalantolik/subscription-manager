using SubscriptionManager.Application.Common.Localization;
using SubscriptionManager.Domain.Subscriptions;

namespace SubscriptionManager.Application.Authentication.RegisterUser;

/// <summary>
/// Request to register a new user.
/// </summary>
public sealed record RegisterUserCommand(
    string Email,
    string Password,
    Language Language,
    Currency BaseCurrency);
