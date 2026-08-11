using SubscriptionManager.Domain.Billing;

namespace SubscriptionManager.Domain.Tests.Billing;

public sealed class BillingSubscriptionChangePolicyTests
{
    [Fact]
    public void GetTiming_ForUpgradeWithSameInterval_ShouldReturnImmediate()
    {
        var result =
            BillingSubscriptionChangePolicy.GetTiming(
                SubscriptionPlan.Plus,
                BillingInterval.Monthly,
                SubscriptionPlan.Premium,
                BillingInterval.Monthly);

        Assert.Equal(
            BillingSubscriptionChangeTiming.Immediate,
            result);
    }

    [Fact]
    public void GetTiming_ForDowngradeWithSameInterval_ShouldReturnNextBillingPeriod()
    {
        var result =
            BillingSubscriptionChangePolicy.GetTiming(
                SubscriptionPlan.Premium,
                BillingInterval.Monthly,
                SubscriptionPlan.Plus,
                BillingInterval.Monthly);

        Assert.Equal(
            BillingSubscriptionChangeTiming
                .NextBillingPeriod,
            result);
    }

    [Theory]
    [InlineData(
        SubscriptionPlan.Plus,
        BillingInterval.Monthly,
        SubscriptionPlan.Plus,
        BillingInterval.Yearly)]
    [InlineData(
        SubscriptionPlan.Plus,
        BillingInterval.Yearly,
        SubscriptionPlan.Plus,
        BillingInterval.Monthly)]
    [InlineData(
        SubscriptionPlan.Plus,
        BillingInterval.Monthly,
        SubscriptionPlan.Premium,
        BillingInterval.Yearly)]
    [InlineData(
        SubscriptionPlan.Premium,
        BillingInterval.Yearly,
        SubscriptionPlan.Plus,
        BillingInterval.Monthly)]
    public void GetTiming_WhenBillingIntervalChanges_ShouldReturnNextBillingPeriod(
        SubscriptionPlan currentPlan,
        BillingInterval currentBillingInterval,
        SubscriptionPlan targetPlan,
        BillingInterval targetBillingInterval)
    {
        var result =
            BillingSubscriptionChangePolicy.GetTiming(
                currentPlan,
                currentBillingInterval,
                targetPlan,
                targetBillingInterval);

        Assert.Equal(
            BillingSubscriptionChangeTiming
                .NextBillingPeriod,
            result);
    }

    [Fact]
    public void GetTiming_WhenSelectionIsAlreadyActive_ShouldThrow()
    {
        var exception =
            Assert.Throws<InvalidOperationException>(() =>
                BillingSubscriptionChangePolicy.GetTiming(
                    SubscriptionPlan.Plus,
                    BillingInterval.Monthly,
                    SubscriptionPlan.Plus,
                    BillingInterval.Monthly));

        Assert.Equal(
            "The selected subscription plan and billing interval are already active.",
            exception.Message);
    }

    [Theory]
    [InlineData(SubscriptionPlan.Free)]
    [InlineData((SubscriptionPlan)999)]
    public void GetTiming_WithInvalidCurrentPlan_ShouldThrow(
        SubscriptionPlan currentPlan)
    {
        var exception =
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                BillingSubscriptionChangePolicy.GetTiming(
                    currentPlan,
                    BillingInterval.Monthly,
                    SubscriptionPlan.Premium,
                    BillingInterval.Monthly));

        Assert.Equal(
            "currentPlan",
            exception.ParamName);
    }

    [Theory]
    [InlineData(SubscriptionPlan.Free)]
    [InlineData((SubscriptionPlan)999)]
    public void GetTiming_WithInvalidTargetPlan_ShouldThrow(
        SubscriptionPlan targetPlan)
    {
        var exception =
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                BillingSubscriptionChangePolicy.GetTiming(
                    SubscriptionPlan.Plus,
                    BillingInterval.Monthly,
                    targetPlan,
                    BillingInterval.Monthly));

        Assert.Equal(
            "targetPlan",
            exception.ParamName);
    }

    [Fact]
    public void GetTiming_WithInvalidCurrentBillingInterval_ShouldThrow()
    {
        var exception =
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                BillingSubscriptionChangePolicy.GetTiming(
                    SubscriptionPlan.Plus,
                    (BillingInterval)999,
                    SubscriptionPlan.Premium,
                    BillingInterval.Monthly));

        Assert.Equal(
            "currentBillingInterval",
            exception.ParamName);
    }

    [Fact]
    public void GetTiming_WithInvalidTargetBillingInterval_ShouldThrow()
    {
        var exception =
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                BillingSubscriptionChangePolicy.GetTiming(
                    SubscriptionPlan.Plus,
                    BillingInterval.Monthly,
                    SubscriptionPlan.Premium,
                    (BillingInterval)999));

        Assert.Equal(
            "targetBillingInterval",
            exception.ParamName);
    }
}
