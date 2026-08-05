using SubscriptionManager.Application.Common.Localization;
using SubscriptionManager.Domain.Subscriptions;

namespace SubscriptionManager.Application.Account.UpdateAccountPreferences;

public sealed record UpdateAccountPreferencesCommand(
    Guid UserId,
    Language Language,
    Currency BaseCurrency);
