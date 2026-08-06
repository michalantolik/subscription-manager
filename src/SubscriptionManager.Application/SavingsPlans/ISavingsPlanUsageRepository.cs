namespace SubscriptionManager.Application.SavingsPlans;

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
