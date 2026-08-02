using SubscriptionManager.Domain.ExchangeRates;

namespace SubscriptionManager.Application.ExchangeRates;

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
