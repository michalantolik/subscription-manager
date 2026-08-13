using SubscriptionManager.Domain.ExchangeRates;

namespace SubscriptionManager.Application.ExchangeRates;

/// <summary>
/// Persistence operations for exchange rates.
/// </summary>
public interface IExchangeRateRepository
{
    Task<IReadOnlyCollection<ExchangeRate>> GetAllAsync(
        CancellationToken cancellationToken = default);

    Task AddRangeAsync(
        IEnumerable<ExchangeRate> exchangeRates,
        CancellationToken cancellationToken = default);

    Task SaveChangesAsync(
        CancellationToken cancellationToken = default);
}
