namespace SubscriptionManager.Application.ExchangeRates;

/// <summary>
/// Provides current exchange rates for currency conversion.
/// </summary>
public interface IExchangeRateService
{
    Task<CurrentExchangeRates> GetCurrentAsync(
        CancellationToken cancellationToken = default);
}
