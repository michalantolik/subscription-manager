using SubscriptionManager.Domain.Subscriptions;

namespace SubscriptionManager.Application.ExchangeRates.ExternalSource;

/// <summary>
/// Exchange rate for a currency relative to PLN, provided by an external source.
/// </summary>
public sealed record ExchangeRateQuote(
    Currency Currency,
    decimal RateToPln);
