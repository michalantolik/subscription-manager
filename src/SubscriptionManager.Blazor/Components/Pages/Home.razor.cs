using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using SubscriptionManager.Blazor.Features.Authentication;
using SubscriptionManager.Blazor.Features.Currencies;
using SubscriptionManager.Blazor.Features.Subscriptions;

namespace SubscriptionManager.Blazor.Components.Pages;

public partial class Home
{
    [CascadingParameter]
    private Task<AuthenticationState> AuthenticationStateTask
    {
        get;
        set;
    } = default!;

    private SubscriptionCostSummaryResponse? _summary;

    private CostGrouping _grouping =
        CostGrouping.Category;

    private string? _activeChartItemKey;

    private bool _loading = true;
    private bool _error;

    private SubscriptionCostSummaryItemResponse? MostExpensive =>
        _summary?.TopSubscriptions.FirstOrDefault();

    private IReadOnlyList<CostChartItem> ChartItems =>
        _grouping == CostGrouping.Category
            ? CreateCategoryItems()
            : CreateSubscriptionItems();

    private IReadOnlyList<OverviewInsight> Insights =>
        CreateInsights();

    private CostChartItem? ActiveChartItem =>
        ChartItems.FirstOrDefault(item =>
            item.Key == _activeChartItemKey);

    protected override async Task OnInitializedAsync()
    {
        State.BaseCurrencyChanged +=
            OnBaseCurrencyChanged;

        await LoadAsync();
    }

    private async Task LoadAsync()
    {
        _loading = true;
        _error = false;

        try
        {
            var authenticationState =
                await AuthenticationStateTask;

            _summary =
                await ApiClient.GetCostSummaryAsync(
                    authenticationState.User);
        }
        catch (HttpRequestException exception)
            when (exception.StatusCode ==
                  System.Net.HttpStatusCode.Unauthorized)
        {
            SessionExpirationNavigation.RedirectToLogin(
                Navigation);
        }
        catch
        {
            _error = true;
        }
        finally
        {
            _loading = false;
        }
    }

    private void SetGrouping(
        CostGrouping grouping)
    {
        _grouping = grouping;
        _activeChartItemKey = null;
    }

    private void ActivateChartItem(
        CostChartItem item)
    {
        _activeChartItemKey = item.Key;
    }

    private void DeactivateChartItem(
        CostChartItem item)
    {
        if (_activeChartItemKey == item.Key)
        {
            _activeChartItemKey = null;
        }
    }

    private string ChartSegmentClass(
        CostChartItem item)
    {
        if (_activeChartItemKey is null)
        {
            return "cost-donut-link";
        }

        return _activeChartItemKey == item.Key
            ? "cost-donut-link is-active"
            : "cost-donut-link is-muted";
    }

    private string LegendItemClass(
        CostChartItem item)
    {
        return _activeChartItemKey == item.Key
            ? "cost-legend-item is-active"
            : "cost-legend-item";
    }

    private static string LegendItemStyle(
        CostChartItem item)
    {
        return $"--cost-color: {item.Color}";
    }

    private IReadOnlyList<CostChartItem>
        CreateCategoryItems()
    {
        if (_summary is null ||
            _summary.MonthlyCost <= 0m)
        {
            return [];
        }

        var categories =
            _summary.Categories
                .Where(category =>
                    category.MonthlyCost > 0m)
                .OrderByDescending(category =>
                    category.MonthlyCost)
                .ToArray();

        var visible =
            categories
                .Take(5)
                .Select((
                    category,
                    index) =>
                    new CostChartItem(
                        $"category:{category.Category}:{category.CustomCategoryName}",
                        CategoryName(category),
                        category.MonthlyCost,
                        SubscriptionColorPalette.GetColor(index),
                        CategoryTargetUrl(category),
                        0m))
                .ToList();

        var remaining =
            categories
                .Skip(5)
                .Sum(category =>
                    category.MonthlyCost);

        if (remaining > 0m)
        {
            visible.Add(
                new CostChartItem(
                    "category:other",
                    T["Overview.CostStructure.Other"],
                    remaining,
                    SubscriptionColorPalette.GetColor(5),
                    "/subscriptions",
                    0m));
        }

        return AddOffsets(visible);
    }

    private IReadOnlyList<CostChartItem>
        CreateSubscriptionItems()
    {
        if (_summary is null ||
            _summary.MonthlyCost <= 0m)
        {
            return [];
        }

        var visible =
            _summary.TopSubscriptions
                .Where(subscription =>
                    subscription.MonthlyCost > 0m)
                .Take(5)
                .Select((
                    subscription,
                    index) =>
                    new CostChartItem(
                        $"subscription:{subscription.Id}",
                        subscription.Name,
                        subscription.MonthlyCost,
                        SubscriptionColorPalette.GetColor(index),
                        $"/subscriptions?subscriptionId={subscription.Id}",
                        0m))
                .ToList();

        var remaining =
            Math.Max(
                0m,
                _summary.MonthlyCost -
                visible.Sum(item =>
                    item.Amount));

        if (remaining > 0.005m)
        {
            visible.Add(
                new CostChartItem(
                    "subscription:other",
                    T["Overview.CostStructure.Other"],
                    remaining,
                    SubscriptionColorPalette.GetColor(5),
                    "/subscriptions",
                    0m));
        }

        return AddOffsets(visible);
    }

    private IReadOnlyList<CostChartItem> AddOffsets(
        IReadOnlyList<CostChartItem> items)
    {
        if (_summary is null ||
            _summary.MonthlyCost <= 0m)
        {
            return [];
        }

        var result =
            new List<CostChartItem>(
                items.Count);

        var offset = 0m;

        foreach (var item in items)
        {
            result.Add(
                item with
                {
                    Offset = offset
                });

            offset +=
                item.Amount /
                _summary.MonthlyCost *
                100m;
        }

        return result;
    }

    private IReadOnlyList<OverviewInsight>
        CreateInsights()
    {
        if (_summary is null ||
            _summary.ActiveCount == 0 ||
            _summary.MonthlyCost <= 0m)
        {
            return [];
        }

        var insights =
            new List<OverviewInsight>(3);

        var mostExpensive =
            _summary.TopSubscriptions
                .FirstOrDefault();

        if (mostExpensive is not null)
        {
            insights.Add(
                new OverviewInsight(
                    "subscriptions",
                    T[
                        "Overview.Insights.Largest.Title",
                        mostExpensive.Name],
                    T[
                        "Overview.Insights.Largest.Description",
                        Percentage(
                            mostExpensive.MonthlyCost),
                        Money(
                            mostExpensive.MonthlyCost)],
                    SubscriptionTargetUrl(mostExpensive)));
        }

        var topThree =
            _summary.TopSubscriptions
                .Take(3)
                .ToArray();

        if (topThree.Length >= 2)
        {
            var topThreeCost =
                topThree.Sum(subscription =>
                    subscription.MonthlyCost);

            insights.Add(
                new OverviewInsight(
                    "chart",
                    T[
                        "Overview.Insights.Concentration.Title",
                        topThree.Length],
                    T[
                        "Overview.Insights.Concentration.Description",
                        Percentage(topThreeCost),
                        Money(topThreeCost)],
                    SubscriptionTargetUrl(topThree)));
        }

        var largestCategory =
            _summary.Categories
                .Where(category =>
                    category.MonthlyCost > 0m)
                .OrderByDescending(category =>
                    category.MonthlyCost)
                .FirstOrDefault();

        if (largestCategory is not null)
        {
            insights.Add(
                new OverviewInsight(
                    "category",
                    T[
                        "Overview.Insights.Category.Title",
                        CategoryName(largestCategory)],
                    T[
                        "Overview.Insights.Category.Description",
                        Percentage(
                            largestCategory.MonthlyCost),
                        Money(
                            largestCategory.MonthlyCost)],
                    CategoryTargetUrl(largestCategory)));
        }

        if (insights.Count < 3 &&
            _summary.TotalCount >
            _summary.ActiveCount)
        {
            var inactiveCount =
                _summary.TotalCount -
                _summary.ActiveCount;

            insights.Add(
                new OverviewInsight(
                    "end",
                    T["Overview.Insights.Inactive.Title"],
                    T[
                        "Overview.Insights.Inactive.Description",
                        inactiveCount],
                    "/subscriptions?status=ended"));
        }

        return insights
            .Take(3)
            .ToArray();
    }

    private string CategoryName(
        SubscriptionCategoryCostSummaryResponse category)
    {
        if (!string.IsNullOrWhiteSpace(
                category.CustomCategoryName))
        {
            return category.CustomCategoryName;
        }

        return T[
            $"Category.{category.Category}"];
    }

    private string CategoryTargetUrl(
        SubscriptionCategoryCostSummaryResponse category)
    {
        if (!string.IsNullOrWhiteSpace(
                category.CustomCategoryName))
        {
            return
                $"/subscriptions?customCategory={Uri.EscapeDataString(category.CustomCategoryName)}";
        }

        return
            $"/subscriptions?category={Uri.EscapeDataString(category.Category)}";
    }

    private static string SubscriptionTargetUrl(
        params SubscriptionCostSummaryItemResponse[] subscriptions)
    {
        if (subscriptions.Length == 0)
        {
            return "/subscriptions";
        }

        var query =
            string.Join(
                "&",
                subscriptions.Select(subscription =>
                    $"subscriptionId={subscription.Id}"));

        return $"/subscriptions?{query}";
    }

    private string SegmentDashArray(
        CostChartItem item)
    {
        if (_summary is null ||
            _summary.MonthlyCost <= 0m)
        {
            return "0 100";
        }

        var percentage =
            item.Amount /
            _summary.MonthlyCost *
            100m;

        var visiblePercentage =
            Math.Max(
                0m,
                percentage - 0.7m);

        return FormattableString.Invariant(
            $"{visiblePercentage:0.###} {100m - visiblePercentage:0.###}");
    }

    private static string SegmentDashOffset(
        CostChartItem item)
    {
        return FormattableString.Invariant(
            $"{-item.Offset:0.###}");
    }

    private string ChartItemLabel(
        CostChartItem item)
    {
        return T[
            "Overview.CostStructure.ItemLabel",
            item.Name,
            Money(item.Amount),
            Percentage(item.Amount)];
    }

    private string TooltipStyle(
        CostChartItem item)
    {
        if (_summary is null ||
            _summary.MonthlyCost <= 0m)
        {
            return $"--tooltip-color: {item.Color}";
        }

        var percentage =
            item.Amount /
            _summary.MonthlyCost *
            100m;

        var angle =
            ((double)(item.Offset + percentage / 2m) * 3.6d - 90d) *
            Math.PI /
            180d;

        var horizontalDirection =
            Math.Cos(angle);

        var verticalDirection =
            Math.Sin(angle);

        var x =
            50d +
            horizontalDirection * 46d;

        var y =
            50d +
            verticalDirection * 46d;

        var translateX = "-50%";
        var translateY = "-112%";

        if (Math.Abs(horizontalDirection) >=
            Math.Abs(verticalDirection))
        {
            if (horizontalDirection < 0d)
            {
                translateX = "-104%";
                translateY = "-50%";
            }
            else
            {
                translateY =
                    verticalDirection < 0d
                        ? "-112%"
                        : "12%";
            }
        }
        else if (verticalDirection > 0d)
        {
            translateY = "12%";
        }

        return FormattableString.Invariant(
            $"--tooltip-x: {x:0.##}%; --tooltip-y: {y:0.##}%; --tooltip-translate-x: {translateX}; --tooltip-translate-y: {translateY}; --tooltip-color: {item.Color}");
    }

    private string Percentage(
        decimal amount)
    {
        if (_summary is null ||
            _summary.MonthlyCost <= 0m)
        {
            return "0%";
        }

        var percentage =
            amount /
            _summary.MonthlyCost;

        return percentage.ToString(
            "P1",
            System.Globalization.CultureInfo.CurrentCulture);
    }

    private string Money(
        decimal value)
    {
        var currency =
            _summary?.BaseCurrency ??
            Currency.PLN;

        return string.Format(
            System.Globalization.CultureInfo.CurrentCulture,
            "{0:N2} {1}",
            value,
            currency);
    }

    private void OnBaseCurrencyChanged()
    {
        _ = InvokeAsync(
            async () =>
            {
                await LoadAsync();
                StateHasChanged();
            });
    }

    public void Dispose()
    {
        State.BaseCurrencyChanged -=
            OnBaseCurrencyChanged;
    }

    private enum CostGrouping
    {
        Category,
        Subscription
    }

    private sealed record CostChartItem(
        string Key,
        string Name,
        decimal Amount,
        string Color,
        string TargetUrl,
        decimal Offset);

    private sealed record OverviewInsight(
        string Icon,
        string Title,
        string Description,
        string TargetUrl);
}
