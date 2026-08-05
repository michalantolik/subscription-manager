using SubscriptionManager.Application.Common.Localization;
using SubscriptionManager.Domain.Subscriptions;

namespace SubscriptionManager.Application.Common.Identity;

public sealed record AccountPreferences(
    Language Language,
    Currency BaseCurrency);
