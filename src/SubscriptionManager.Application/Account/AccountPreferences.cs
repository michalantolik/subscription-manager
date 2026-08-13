using SubscriptionManager.Application.Common.Localization;
using SubscriptionManager.Domain.Subscriptions;

namespace SubscriptionManager.Application.Account;

/// <summary>
/// User account preferences.
/// </summary>
public sealed record AccountPreferences(
    Language Language,
    Currency BaseCurrency);
