using SubscriptionManager.Web.Common.Currencies;
using SubscriptionManager.Web.Common.Localization;

namespace SubscriptionManager.Web.Features.Account;

/// <summary>
/// Account preferences used by the web application.
/// </summary>
public sealed record AccountPreferences(
    Language Language,
    Currency BaseCurrency);
