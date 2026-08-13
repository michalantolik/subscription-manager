using SubscriptionManager.Domain.DigitalServices;
using SubscriptionManager.Domain.Subscriptions;

namespace SubscriptionManager.Application.Subscriptions;

/// <summary>
/// Subscription data returned by subscription use cases.
/// </summary>
public sealed record SubscriptionDto(
    Guid Id,
    Guid? DigitalServiceId,
    string Name,
    DigitalServiceCategory? Category,
    string? CustomCategoryName,
    string? IconKey,
    string? ManagementUrl,
    decimal Amount,
    Currency Currency,
    BillingPeriod BillingPeriod,
    DateOnly StartDate,
    DateOnly? EndDate,
    bool IsActive,
    decimal MonthlyEquivalentAmount,
    decimal YearlyEquivalentAmount);
