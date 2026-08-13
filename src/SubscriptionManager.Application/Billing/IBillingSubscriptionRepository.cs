using SubscriptionManager.Domain.Billing;

namespace SubscriptionManager.Application.Billing;

/// <summary>
/// Persistence operations for billing subscription use cases.
/// </summary>
public interface IBillingSubscriptionRepository
{
    Task AddAsync(
        BillingSubscription subscription,
        CancellationToken cancellationToken = default);

    Task<BillingSubscription?> GetByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    Task<BillingSubscription?> GetByProviderSubscriptionIdAsync(
        string providerSubscriptionId,
        CancellationToken cancellationToken = default);

    Task SaveChangesAsync(
        CancellationToken cancellationToken = default);
}
