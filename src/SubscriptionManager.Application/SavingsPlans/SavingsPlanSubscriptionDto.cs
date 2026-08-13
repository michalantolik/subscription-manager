namespace SubscriptionManager.Application.SavingsPlans;

/// <summary>
/// Subscription data included in savings plan scenarios.
/// </summary>
public sealed record SavingsPlanSubscriptionDto(
    Guid Id,
    string Name,
    string Category,
    decimal MonthlyCost);
