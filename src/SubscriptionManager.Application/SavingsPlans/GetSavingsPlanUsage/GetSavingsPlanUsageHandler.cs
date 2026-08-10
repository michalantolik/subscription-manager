using SubscriptionManager.Application.Common.Identity;
using SubscriptionManager.Domain.Billing;

namespace SubscriptionManager.Application.SavingsPlans.GetSavingsPlanUsage;

public sealed class GetSavingsPlanUsageHandler
{
    private readonly IIdentityService _identityService;
    private readonly ICurrentUser _currentUser;
    private readonly ISavingsPlanUsageRepository
        _savingsPlanUsageRepository;

    public GetSavingsPlanUsageHandler(
        IIdentityService identityService,
        ICurrentUser currentUser,
        ISavingsPlanUsageRepository savingsPlanUsageRepository)
    {
        _identityService = identityService;
        _currentUser = currentUser;
        _savingsPlanUsageRepository =
            savingsPlanUsageRepository;
    }

    public async Task<SavingsPlanUsageDto> HandleAsync(
        CancellationToken cancellationToken = default)
    {
        var userId =
            _currentUser.UserId;

        var subscriptionPlan =
            await _identityService.GetSubscriptionPlanAsync(
                userId,
                cancellationToken);

        if (subscriptionPlan is null)
        {
            throw new InvalidOperationException(
                "The current user's subscription plan is unavailable.");
        }

        if (!SubscriptionPlanLimits.CanUseSavingsPlan(
                subscriptionPlan.Value))
        {
            return new SavingsPlanUsageDto(
                subscriptionPlan.Value,
                0,
                0);
        }

        var dailyRequestLimit =
            SubscriptionPlanLimits.GetDailySavingsPlanLimit(
                subscriptionPlan.Value);

        var usageDateUtc =
            DateOnly.FromDateTime(
                DateTime.UtcNow);

        var remainingRequestCount =
            await _savingsPlanUsageRepository
                .GetRemainingRequestCountAsync(
                    userId,
                    usageDateUtc,
                    dailyRequestLimit,
                    cancellationToken);

        return new SavingsPlanUsageDto(
            subscriptionPlan.Value,
            dailyRequestLimit,
            remainingRequestCount);
    }
}

public sealed record SavingsPlanUsageDto(
    SubscriptionPlan SubscriptionPlan,
    int DailyRequestLimit,
    int RemainingRequestCount);
