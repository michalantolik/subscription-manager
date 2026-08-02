using SubscriptionManager.Domain.DigitalServices;
using SubscriptionManager.Domain.Subscriptions;

namespace SubscriptionManager.Application.Subscriptions;

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
    IReadOnlyCollection<SubscriptionCategoryCostSummaryDto> Categories);

public sealed record SubscriptionCostSummaryItemDto(
    Guid Id,
    string Name,
    BillingPeriod BillingPeriod,
    decimal MonthlyCost);

public sealed record SubscriptionCategoryCostSummaryDto(
    DigitalServiceCategory Category,
    string? CustomCategoryName,
    decimal MonthlyCost);
