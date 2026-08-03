namespace SubscriptionManager.Application.SavingsPlans.CreateSavingsPlan;

public sealed record CreateSavingsPlanCommand(
    SavingsPlanGoalType GoalType,
    decimal TargetAmount,
    IReadOnlyCollection<Guid> ProtectedSubscriptionIds,
    SavingsPlanStrategy Strategy,
    string? AdditionalPreference);
