using SubscriptionManager.Application.SavingsPlans;
using SubscriptionManager.Domain.Subscriptions;
using SubscriptionManager.Infrastructure.SavingsPlans;

namespace SubscriptionManager.Infrastructure.Tests.SavingsPlans;

public sealed class SavingsPlanToolsTests
{
    [Fact]
    public void GetAvailableSubscriptions_ShouldExcludeProtectedSubscriptions()
    {
        var netflix =
            CreateSubscription(
                "Netflix",
                60m);

        var spotify =
            CreateSubscription(
                "Spotify",
                40m);

        var request =
            CreateRequest(
                [netflix, spotify],
                [spotify.Id]);

        var tools =
            new SavingsPlanTools(request);

        var result =
            tools.GetAvailableSubscriptions();

        var availableSubscription =
            Assert.Single(result);

        Assert.Equal(
            netflix.Id,
            availableSubscription.Id);
    }

    [Fact]
    public void SimulateEndingSubscriptions_ShouldCalculateExactResult()
    {
        var netflix =
            CreateSubscription(
                "Netflix",
                60m);

        var spotify =
            CreateSubscription(
                "Spotify",
                40m);

        var request =
            CreateRequest(
                [netflix, spotify],
                []);

        var tools =
            new SavingsPlanTools(request);

        var result =
            tools.SimulateEndingSubscriptions(
                [netflix.Id]);

        Assert.True(
            result.IsValid);

        Assert.Null(
            result.Error);

        Assert.Equal(
            40m,
            result.ProjectedMonthlyCost);

        Assert.Equal(
            60m,
            result.MonthlySavings);

        Assert.True(
            result.TargetReached);

        Assert.Equal(
            netflix.Id,
            Assert.Single(result.SubscriptionIds));
    }

    [Fact]
    public void SimulateEndingSubscriptions_ShouldRejectProtectedSubscription()
    {
        var netflix =
            CreateSubscription(
                "Netflix",
                60m);

        var spotify =
            CreateSubscription(
                "Spotify",
                40m);

        var request =
            CreateRequest(
                [netflix, spotify],
                [spotify.Id]);

        var tools =
            new SavingsPlanTools(request);

        var result =
            tools.SimulateEndingSubscriptions(
                [spotify.Id]);

        Assert.False(
            result.IsValid);

        Assert.Equal(
            "A subscription is unavailable or protected.",
            result.Error);

        Assert.Empty(
            result.SubscriptionIds);

        Assert.Equal(
            100m,
            result.ProjectedMonthlyCost);

        Assert.Equal(
            0m,
            result.MonthlySavings);

        Assert.False(
            result.TargetReached);
    }

    private static SavingsPlanAgentRequest CreateRequest(
        IReadOnlyCollection<SavingsPlanSubscriptionDto> subscriptions,
        IReadOnlyCollection<Guid> protectedSubscriptionIds)
    {
        return new SavingsPlanAgentRequest(
            SavingsPlanGoalType.MonthlyBudget,
            50m,
            SavingsPlanStrategy.Balanced,
            null,
            "en",
            Currency.PLN,
            100m,
            protectedSubscriptionIds,
            subscriptions);
    }

    private static SavingsPlanSubscriptionDto CreateSubscription(
        string name,
        decimal monthlyCost)
    {
        return new SavingsPlanSubscriptionDto(
            Guid.NewGuid(),
            name,
            "Entertainment",
            monthlyCost);
    }
}
