using System.ComponentModel;
using SubscriptionManager.Application.SavingsPlans;
using SubscriptionManager.Application.SavingsPlans.Ai;

namespace SubscriptionManager.Infrastructure.SavingsPlans;

internal sealed class SavingsPlanTools
{
    private readonly SavingsPlanAgentRequest _request;

    private readonly IReadOnlyDictionary<
        Guid,
        SavingsPlanSubscriptionDto> _availableSubscriptions;

    public SavingsPlanTools(
        SavingsPlanAgentRequest request)
    {
        _request = request;

        _availableSubscriptions =
            request.Subscriptions
                .Where(subscription =>
                    !request.ProtectedSubscriptionIds.Contains(
                        subscription.Id))
                .ToDictionary(
                    subscription => subscription.Id);
    }

    [Description(
        "Returns active subscriptions that may be considered for ending. Protected subscriptions are excluded.")]
    public IReadOnlyCollection<SavingsPlanSubscriptionDto>
        GetAvailableSubscriptions()
    {
        return _availableSubscriptions.Values.ToArray();
    }

    [Description(
        "Calculates the exact financial result of ending the selected subscriptions. Use only identifiers returned by GetAvailableSubscriptions.")]
    public SavingsPlanSimulationResult SimulateEndingSubscriptions(
        [Description(
            "Identifiers of subscriptions included in the simulated scenario.")]
        IReadOnlyCollection<Guid> subscriptionIds)
    {
        if (subscriptionIds is null ||
            subscriptionIds.Count == 0)
        {
            return InvalidSimulation(
                "At least one subscription must be selected.");
        }

        var distinctIds =
            subscriptionIds
                .Distinct()
                .ToArray();

        if (distinctIds.Any(
                id => !_availableSubscriptions.ContainsKey(id)))
        {
            return InvalidSimulation(
                "A subscription is unavailable or protected.");
        }

        var monthlySavings =
            distinctIds.Sum(
                id =>
                    _availableSubscriptions[id].MonthlyCost);

        var projectedMonthlyCost =
            Math.Max(
                0m,
                _request.CurrentMonthlyCost -
                monthlySavings);

        var targetReached =
            _request.GoalType switch
            {
                SavingsPlanGoalType.MonthlyBudget =>
                    projectedMonthlyCost <=
                    _request.TargetAmount,

                SavingsPlanGoalType.MonthlySavings =>
                    monthlySavings >=
                    _request.TargetAmount,

                _ => false
            };

        return new SavingsPlanSimulationResult(
            true,
            null,
            distinctIds,
            projectedMonthlyCost,
            monthlySavings,
            targetReached);
    }

    private SavingsPlanSimulationResult InvalidSimulation(
        string error)
    {
        return new SavingsPlanSimulationResult(
            false,
            error,
            [],
            _request.CurrentMonthlyCost,
            0m,
            false);
    }
}
