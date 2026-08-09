using SubscriptionManager.Application.Common.Identity;
using SubscriptionManager.Domain.Billing;

namespace SubscriptionManager.Application.Tests.Common.Identity;

public sealed class SubscriptionPlanLimitsTests
{
    [Theory]
    [InlineData(
        SubscriptionPlan.Free,
        SubscriptionPlanLimits.FreeDailySavingsPlanLimit)]
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
}
