using SubscriptionManager.Application.Common.Localization;
using SubscriptionManager.Domain.Subscriptions;

namespace SubscriptionManager.Application.Account.UpdateAccountPreferences;

/// <summary>
/// Request to update user account preferences.
/// </summary>
public sealed record UpdateAccountPreferencesCommand(
    Guid UserId,
    Language Language,
    Currency BaseCurrency);
