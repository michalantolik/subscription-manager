using SubscriptionManager.Domain.DigitalServices;
using SubscriptionManager.Domain.Subscriptions;

namespace SubscriptionManager.Application.Subscriptions;

/// <summary>
/// Subscription cost summary data returned by subscription use cases.
/// </summary>
public sealed record SubscriptionCostSummaryDto(
    Currency BaseCurrency,
    DateOnly? ExchangeRateEffectiveDate,
    int ActiveCount,
    int TotalCount,
    decimal MonthlyCost,
    decimal YearlyCost,
    decimal AverageMonthlyCost,
    decimal AverageYearlyCost,
    IReadOnlyCollection<SubscriptionCostSummaryItemDto> TopSubscriptions,
    IReadOnlyCollection<SubscriptionCostSummaryItemDto> ActiveSubscriptions,
    IReadOnlyCollection<SubscriptionCategoryCostSummaryDto> Categories);

/// <summary>
/// Subscription cost data included in subscription cost summaries.
/// </summary>
public sealed record SubscriptionCostSummaryItemDto(
    Guid Id,
    string Name,
    BillingPeriod BillingPeriod,
    decimal MonthlyCost);

/// <summary>
/// Subscription category cost data included in subscription cost summaries.
/// </summary>
public sealed record SubscriptionCategoryCostSummaryDto(
    DigitalServiceCategory Category,
    string? CustomCategoryName,
    decimal MonthlyCost);
