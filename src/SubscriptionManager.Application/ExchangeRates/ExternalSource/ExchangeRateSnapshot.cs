namespace SubscriptionManager.Application.ExchangeRates.ExternalSource;

/// <summary>
/// Exchange rates provided by an external source for a specific date.
/// </summary>
public sealed record ExchangeRateSnapshot(
    DateOnly EffectiveDate,
    IReadOnlyCollection<ExchangeRateQuote> Rates);
