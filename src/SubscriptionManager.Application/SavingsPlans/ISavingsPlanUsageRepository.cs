namespace SubscriptionManager.Application.SavingsPlans;

/// <summary>
/// Persistence operations for savings plan usage.
/// </summary>
public interface ISavingsPlanUsageRepository
{
    Task<int> GetRemainingRequestCountAsync(
        Guid userId,
        DateOnly usageDateUtc,
        int dailyLimit,
        CancellationToken cancellationToken = default);

    Task<int?> TryRegisterRequestAsync(
        Guid userId,
        DateOnly usageDateUtc,
        int dailyLimit,
        CancellationToken cancellationToken = default);
}
