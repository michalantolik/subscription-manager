using System.Text.Json.Serialization;
using SubscriptionManager.Web.Common.Currencies;

namespace SubscriptionManager.Web.Features.Subscriptions;

/// <summary>
/// Represents a billing period supported for subscriptions.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum BillingPeriod
{
    Monthly = 1,
    Quarterly = 2,
    SemiAnnual = 3,
    Yearly = 4
}

/// <summary>
/// Subscription data returned by the Subscriptions API.
/// </summary>
public sealed record SubscriptionResponse(
    Guid Id,
    Guid? DigitalServiceId,
    string Name,
    string? Category,
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

/// <summary>
/// Subscription cost summary returned by the Subscriptions API.
/// </summary>
public sealed record SubscriptionCostSummaryResponse(
    Currency BaseCurrency,
    DateOnly? ExchangeRateEffectiveDate,
    int ActiveCount,
    int TotalCount,
    decimal MonthlyCost,
    decimal YearlyCost,
    decimal AverageMonthlyCost,
    decimal AverageYearlyCost,
    IReadOnlyList<SubscriptionCostSummaryItemResponse>
        TopSubscriptions,
    IReadOnlyList<SubscriptionCostSummaryItemResponse>
        ActiveSubscriptions,
    IReadOnlyList<SubscriptionCategoryCostSummaryResponse>
        Categories);

/// <summary>
/// Subscription cost data returned as part of a cost summary.
/// </summary>
public sealed record SubscriptionCostSummaryItemResponse(
    Guid Id,
    string Name,
    BillingPeriod BillingPeriod,
    decimal MonthlyCost);

/// <summary>
/// Subscription category cost data returned as part of a cost summary.
/// </summary>
public sealed record SubscriptionCategoryCostSummaryResponse(
    string Category,
    string? CustomCategoryName,
    decimal MonthlyCost);
