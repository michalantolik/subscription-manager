using SubscriptionManager.Domain.Subscriptions;

namespace SubscriptionManager.Application.SavingsPlans;

public sealed record SavingsPlanAgentRequest(
    SavingsPlanGoalType GoalType,
    decimal TargetAmount,
    SavingsPlanStrategy Strategy,
    string? AdditionalPreference,
    string LanguageCode,
    Currency BaseCurrency,
    decimal CurrentMonthlyCost,
    IReadOnlyCollection<Guid> ProtectedSubscriptionIds,
    IReadOnlyCollection<SavingsPlanSubscriptionDto> Subscriptions);
