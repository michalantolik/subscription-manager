using SubscriptionManager.Domain.Subscriptions;

namespace SubscriptionManager.Application.Subscriptions;

public sealed record SubscriptionDto(
    Guid Id,
    string Name,
    decimal Amount,
    string Currency,
    BillingPeriod BillingPeriod,
    DateOnly StartDate,
    DateOnly? EndDate,
    bool IsActive,
    decimal MonthlyEquivalentAmount,
    decimal YearlyEquivalentAmount);
