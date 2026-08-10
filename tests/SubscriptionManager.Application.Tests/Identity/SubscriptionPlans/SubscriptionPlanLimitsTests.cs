using SubscriptionManager.Application.Common.Identity;
using SubscriptionManager.Domain.Billing;

namespace SubscriptionManager.Application.Tests.Common.Identity;

public sealed class SubscriptionPlanLimitsTests
{
    [Theory]
    [InlineData(
        SubscriptionPlan.Plus,
        SubscriptionPlanLimits.PlusDailySavingsPlanLimit)]
    [InlineData(
        SubscriptionPlan.Premium,
        SubscriptionPlanLimits.PremiumDailySavingsPlanLimit)]
    public void GetDailySavingsPlanLimit_ShouldReturnConfiguredLimit(
        SubscriptionPlan subscriptionPlan,
        int expectedLimit)
    {
        var result =
            SubscriptionPlanLimits.GetDailySavingsPlanLimit(
                subscriptionPlan);

        Assert.Equal(
            expectedLimit,
            result);
    }

    [Fact]
    public void GetDailySavingsPlanLimit_ShouldReturnZeroForFreePlan()
    {
        var result =
            SubscriptionPlanLimits.GetDailySavingsPlanLimit(
                SubscriptionPlan.Free);

        Assert.Equal(
            0,
            result);
    }

    [Theory]
    [InlineData(
        SubscriptionPlan.Free,
        false)]
    [InlineData(
        SubscriptionPlan.Plus,
        true)]
    [InlineData(
        SubscriptionPlan.Premium,
        true)]
    public void CanUseSavingsPlan_ShouldReturnExpectedAccess(
        SubscriptionPlan subscriptionPlan,
        bool expected)
    {
        var result =
            SubscriptionPlanLimits.CanUseSavingsPlan(
                subscriptionPlan);

        Assert.Equal(
            expected,
            result);
    }

    [Theory]
    [InlineData(
        SubscriptionPlan.Free,
        SubscriptionPlanLimits.FreeSubscriptionLimit)]
    [InlineData(
        SubscriptionPlan.Plus,
        null)]
    [InlineData(
        SubscriptionPlan.Premium,
        null)]
    public void GetSubscriptionLimit_ShouldReturnConfiguredLimit(
        SubscriptionPlan subscriptionPlan,
        int? expectedLimit)
    {
        var result =
            SubscriptionPlanLimits.GetSubscriptionLimit(
                subscriptionPlan);

        Assert.Equal(
            expectedLimit,
            result);
    }

    [Fact]
    public void GetDailySavingsPlanLimit_ShouldRejectUnsupportedPlan()
    {
        var unsupportedPlan =
            (SubscriptionPlan)999;

        Assert.Throws<ArgumentOutOfRangeException>(
            () =>
                SubscriptionPlanLimits
                    .GetDailySavingsPlanLimit(
                        unsupportedPlan));
    }

    [Fact]
    public void CanUseSavingsPlan_ShouldRejectUnsupportedPlan()
    {
        var unsupportedPlan =
            (SubscriptionPlan)999;

        Assert.Throws<ArgumentOutOfRangeException>(
            () =>
                SubscriptionPlanLimits
                    .CanUseSavingsPlan(
                        unsupportedPlan));
    }

    [Fact]
    public void GetSubscriptionLimit_ShouldRejectUnsupportedPlan()
    {
        var unsupportedPlan =
            (SubscriptionPlan)999;

        Assert.Throws<ArgumentOutOfRangeException>(
            () =>
                SubscriptionPlanLimits
                    .GetSubscriptionLimit(
                        unsupportedPlan));
    }
}
