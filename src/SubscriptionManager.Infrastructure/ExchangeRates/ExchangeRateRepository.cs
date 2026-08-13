using Microsoft.EntityFrameworkCore;
using SubscriptionManager.Application.ExchangeRates;
using SubscriptionManager.Domain.ExchangeRates;
using SubscriptionManager.Infrastructure.Persistence;

namespace SubscriptionManager.Infrastructure.ExchangeRates;

/// <summary>
/// Provides persistence for exchange rates.
/// </summary>
internal sealed class ExchangeRateRepository
    : IExchangeRateRepository
{
    private readonly SubscriptionManagerDbContext _dbContext;

    public ExchangeRateRepository(
        SubscriptionManagerDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyCollection<ExchangeRate>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.ExchangeRates
            .ToListAsync(cancellationToken);
    }

    public async Task AddRangeAsync(
        IEnumerable<ExchangeRate> exchangeRates,
        CancellationToken cancellationToken = default)
    {
        await _dbContext.ExchangeRates.AddRangeAsync(
            exchangeRates,
            cancellationToken);
    }

    public async Task SaveChangesAsync(
        CancellationToken cancellationToken = default)
    {
        await _dbContext.SaveChangesAsync(
            cancellationToken);
    }
}
