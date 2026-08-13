using SubscriptionManager.Application.Common.Identity;
using SubscriptionManager.Application.ExchangeRates;
using SubscriptionManager.Domain.DigitalServices;
using SubscriptionManager.Domain.Subscriptions;

namespace SubscriptionManager.Application.Subscriptions.GetSubscriptionCostSummary;

/// <summary>
/// Handles subscription cost summary retrieval.
/// </summary>
public sealed class GetSubscriptionCostSummaryHandler
{
    private readonly ISubscriptionRepository _subscriptionRepository;
    private readonly IIdentityService _identityService;
    private readonly IExchangeRateService _exchangeRateService;
    private readonly ICurrentUser _currentUser;

    public GetSubscriptionCostSummaryHandler(
        ISubscriptionRepository subscriptionRepository,
        IIdentityService identityService,
        IExchangeRateService exchangeRateService,
        ICurrentUser currentUser)
    {
        _subscriptionRepository = subscriptionRepository;
        _identityService = identityService;
        _exchangeRateService = exchangeRateService;
        _currentUser = currentUser;
    }

    public async Task<SubscriptionCostSummaryDto> HandleAsync(
        CancellationToken cancellationToken = default)
    {
        var subscriptions =
            await _subscriptionRepository.GetAllAsync(
                _currentUser.UserId,
                cancellationToken);

        var baseCurrency =
            await _identityService.GetBaseCurrencyAsync(
                _currentUser.UserId,
                cancellationToken);

        if (baseCurrency is null)
        {
            throw new InvalidOperationException(
                "The current user's base currency is unavailable.");
        }

        var activeSubscriptions =
            subscriptions
                .Where(subscription =>
                    subscription.IsActive)
                .ToArray();

        if (activeSubscriptions.Length == 0)
        {
            return CreateEmptySummary(
                baseCurrency.Value,
                subscriptions.Count);
        }

        CurrentExchangeRates? exchangeRates = null;

        if (activeSubscriptions.Any(subscription =>
                subscription.Currency != baseCurrency.Value))
        {
            exchangeRates =
                await _exchangeRateService.GetCurrentAsync(
                    cancellationToken);
        }

        var costs =
            activeSubscriptions
                .Select(subscription =>
                    CreateCost(
                        subscription,
                        baseCurrency.Value,
                        exchangeRates))
                .ToArray();

        var monthlyCost =
            costs.Sum(cost =>
                cost.MonthlyCost);

        var yearlyCost =
            costs.Sum(cost =>
                cost.YearlyCost);

        var activeSubscriptionItems =
            costs
                .OrderByDescending(cost =>
                    cost.MonthlyCost)
                .ThenBy(cost =>
                    cost.Subscription.Name)
                .Select(cost =>
                    new SubscriptionCostSummaryItemDto(
                        cost.Subscription.Id,
                        cost.Subscription.Name,
                        cost.Subscription.BillingPeriod,
                        cost.MonthlyCost))
                .ToArray();

        var topSubscriptions =
            activeSubscriptionItems
                .Take(5)
                .ToArray();

        var categories =
            costs
                .GroupBy(cost => new
                {
                    Category =
                        cost.Subscription.Category ??
                        DigitalServiceCategory.Other,

                    CustomCategoryName =
                        NormalizeCustomCategoryName(
                            cost.Subscription
                                .CustomCategoryName)
                })
                .Select(group =>
                    new SubscriptionCategoryCostSummaryDto(
                        group.Key.Category,
                        group.Key.CustomCategoryName,
                        group.Sum(cost =>
                            cost.MonthlyCost)))
                .OrderByDescending(category =>
                    category.MonthlyCost)
                .ThenBy(category =>
                    category.Category)
                .ThenBy(category =>
                    category.CustomCategoryName)
                .ToArray();

        return new SubscriptionCostSummaryDto(
            baseCurrency.Value,
            exchangeRates?.EffectiveDate,
            activeSubscriptions.Length,
            subscriptions.Count,
            monthlyCost,
            yearlyCost,
            monthlyCost / activeSubscriptions.Length,
            yearlyCost / activeSubscriptions.Length,
            topSubscriptions,
            activeSubscriptionItems,
            categories);
    }

    private static SubscriptionCost CreateCost(
        Subscription subscription,
        Currency baseCurrency,
        CurrentExchangeRates? exchangeRates)
    {
        if (subscription.Currency == baseCurrency)
        {
            return new SubscriptionCost(
                subscription,
                subscription.MonthlyEquivalentAmount,
                subscription.YearlyEquivalentAmount);
        }

        if (exchangeRates is null)
        {
            throw new InvalidOperationException(
                "Current exchange rates are unavailable.");
        }

        return new SubscriptionCost(
            subscription,
            exchangeRates.Convert(
                subscription.MonthlyEquivalentAmount,
                subscription.Currency,
                baseCurrency),
            exchangeRates.Convert(
                subscription.YearlyEquivalentAmount,
                subscription.Currency,
                baseCurrency));
    }

    private static SubscriptionCostSummaryDto CreateEmptySummary(
        Currency baseCurrency,
        int totalCount)
    {
        return new SubscriptionCostSummaryDto(
            baseCurrency,
            null,
            0,
            totalCount,
            0m,
            0m,
            0m,
            0m,
            [],
            [],
            []);
    }

    private static string? NormalizeCustomCategoryName(
        string? customCategoryName)
    {
        return string.IsNullOrWhiteSpace(
            customCategoryName)
            ? null
            : customCategoryName.Trim();
    }

    private sealed record SubscriptionCost(
        Subscription Subscription,
        decimal MonthlyCost,
        decimal YearlyCost);
}
