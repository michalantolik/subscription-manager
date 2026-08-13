namespace SubscriptionManager.Application.SavingsPlans.CreateSavingsPlan;

/// <summary>
/// Request to create a savings plan.
/// </summary>
public sealed record CreateSavingsPlanCommand(
    SavingsPlanGoalType GoalType,
    decimal TargetAmount,
    IReadOnlyCollection<Guid> ProtectedSubscriptionIds,
    SavingsPlanStrategy Strategy,
    string? AdditionalPreference,
    string LanguageCode);
