namespace SubscriptionManager.Application.SavingsPlans;

public sealed record SavingsPlanSubscriptionDto(
    Guid Id,
    string Name,
    string Category,
    decimal MonthlyCost);
