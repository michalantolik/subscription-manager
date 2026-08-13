namespace SubscriptionManager.Application.ExchangeRates.ExternalSource;

/// <summary>
/// Provides the latest exchange rates from an external source.
/// </summary>
public interface IExchangeRateProvider
{
    Task<ExchangeRateSnapshot> GetLatestAsync(
        CancellationToken cancellationToken = default);
}
