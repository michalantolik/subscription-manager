using Microsoft.EntityFrameworkCore;
using SubscriptionManager.Application.Billing;
using SubscriptionManager.Domain.Billing;

namespace SubscriptionManager.Infrastructure.Persistence.Repositories;

internal sealed class BillingSubscriptionRepository
    : IBillingSubscriptionRepository
{
    private readonly SubscriptionManagerDbContext _dbContext;

    public BillingSubscriptionRepository(
        SubscriptionManagerDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(
        BillingSubscription subscription,
        CancellationToken cancellationToken = default)
    {
        await _dbContext.BillingSubscriptions.AddAsync(
            subscription,
            cancellationToken);
    }

    public async Task<BillingSubscription?> GetByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.BillingSubscriptions
            .SingleOrDefaultAsync(
                subscription =>
                    subscription.UserId == userId,
                cancellationToken);
    }

    public async Task<BillingSubscription?> GetByProviderSubscriptionIdAsync(
        string providerSubscriptionId,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(
            providerSubscriptionId);

        return await _dbContext.BillingSubscriptions
            .SingleOrDefaultAsync(
                subscription =>
                    subscription.ProviderSubscriptionId ==
                    providerSubscriptionId,
                cancellationToken);
    }

    public async Task SaveChangesAsync(
        CancellationToken cancellationToken = default)
    {
        await _dbContext.SaveChangesAsync(
            cancellationToken);
    }
}
