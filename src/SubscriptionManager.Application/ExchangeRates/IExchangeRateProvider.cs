using SubscriptionManager.Domain.Subscriptions;

namespace SubscriptionManager.Application.ExchangeRates;

public interface IExchangeRateProvider
{
    Task<ExchangeRateSnapshot> GetLatestAsync(
        CancellationToken cancellationToken = default);
}

public sealed record ExchangeRateSnapshot(
    DateOnly EffectiveDate,
    IReadOnlyCollection<ExchangeRateQuote> Rates);

public sealed record ExchangeRateQuote(
    Currency Currency,
    decimal RateToPln);
