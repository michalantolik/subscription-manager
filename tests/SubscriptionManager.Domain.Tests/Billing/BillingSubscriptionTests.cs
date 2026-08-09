using SubscriptionManager.Domain.Billing;

namespace SubscriptionManager.Domain.Tests.Billing;

public sealed class BillingSubscriptionTests
{
    [Fact]
    public void Create_ShouldCreateActiveSubscription()
    {
        var periodStart = new DateTimeOffset(
            2026, 8, 9, 0, 0, 0, TimeSpan.Zero);

        var periodEnd = periodStart.AddMonths(1);

        var subscription = new BillingSubscription(
            Guid.NewGuid(),
            Guid.NewGuid(),
            SubscriptionPlan.Plus,
            BillingInterval.Monthly,
            periodStart,
            periodEnd);

        Assert.Equal(SubscriptionPlan.Plus, subscription.Plan);
        Assert.Equal(BillingInterval.Monthly, subscription.BillingInterval);
        Assert.Equal(BillingSubscriptionStatus.Active, subscription.Status);
        Assert.Equal(periodStart, subscription.CurrentPeriodStart);
        Assert.Equal(periodEnd, subscription.CurrentPeriodEnd);
        Assert.False(subscription.CancelAtPeriodEnd);
    }

    [Fact]
    public void Cancel_ShouldKeepSubscriptionUntilPeriodEnd()
    {
        var periodStart = new DateTimeOffset(
            2026, 8, 9, 0, 0, 0, TimeSpan.Zero);

        var periodEnd = periodStart.AddMonths(1);

        var subscription = new BillingSubscription(
            Guid.NewGuid(),
            Guid.NewGuid(),
            SubscriptionPlan.Plus,
            BillingInterval.Monthly,
            periodStart,
            periodEnd);

        subscription.Cancel();

        Assert.Equal(BillingSubscriptionStatus.Canceled, subscription.Status);
        Assert.True(subscription.CancelAtPeriodEnd);
        Assert.Equal(periodEnd, subscription.CurrentPeriodEnd);
    }

    [Fact]
    public void Create_WhenPeriodEndIsNotAfterStart_ShouldThrow()
    {
        var periodStart = new DateTimeOffset(
            2026, 8, 9, 0, 0, 0, TimeSpan.Zero);

        var exception = Assert.Throws<ArgumentException>(() =>
            new BillingSubscription(
                Guid.NewGuid(),
                Guid.NewGuid(),
                SubscriptionPlan.Plus,
                BillingInterval.Monthly,
                periodStart,
                periodStart));

        Assert.Equal(
            "currentPeriodEnd",
            exception.ParamName);
    }
}
