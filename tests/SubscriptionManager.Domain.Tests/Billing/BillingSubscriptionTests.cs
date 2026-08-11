using SubscriptionManager.Domain.Billing;

namespace SubscriptionManager.Domain.Tests.Billing;

public sealed class BillingSubscriptionTests
{
    [Fact]
    public void Create_ShouldCreateActiveSubscription()
    {
        var periodStart = new DateTimeOffset(
            2026, 8, 9, 0, 0, 0, TimeSpan.Zero);

        var periodEnd =
            periodStart.AddMonths(1);

        var subscription =
            new BillingSubscription(
                Guid.NewGuid(),
                Guid.NewGuid(),
                SubscriptionPlan.Plus,
                BillingInterval.Monthly,
                periodStart,
                periodEnd);

        Assert.Equal(
            SubscriptionPlan.Plus,
            subscription.Plan);

        Assert.Equal(
            BillingInterval.Monthly,
            subscription.BillingInterval);

        Assert.Equal(
            BillingSubscriptionStatus.Active,
            subscription.Status);

        Assert.Equal(
            periodStart,
            subscription.CurrentPeriodStart);

        Assert.Equal(
            periodEnd,
            subscription.CurrentPeriodEnd);

        Assert.Null(
            subscription.LastProviderEventCreatedAt);

        Assert.False(
            subscription.CancelAtPeriodEnd);
    }

    [Fact]
    public void LinkToPaymentProvider_ShouldStoreProviderIdentifiers()
    {
        var subscription =
            CreateSubscription();

        subscription.LinkToPaymentProvider(
            "cus_123",
            "sub_123",
            "price_123");

        Assert.Equal(
            "cus_123",
            subscription.ProviderCustomerId);

        Assert.Equal(
            "sub_123",
            subscription.ProviderSubscriptionId);

        Assert.Equal(
            "price_123",
            subscription.ProviderPriceId);
    }

    [Fact]
    public void Synchronize_ShouldUpdateSubscriptionFromPaymentProvider()
    {
        var subscription =
            CreateSubscription();

        subscription.LinkToPaymentProvider(
            "cus_123",
            "sub_123",
            "price_plus_monthly");

        var periodStart = new DateTimeOffset(
            2026, 9, 9, 0, 0, 0, TimeSpan.Zero);

        var periodEnd =
            periodStart.AddYears(1);

        subscription.Synchronize(
            SubscriptionPlan.Premium,
            BillingInterval.Yearly,
            BillingSubscriptionStatus.PastDue,
            "price_premium_yearly",
            periodStart,
            periodEnd,
            true);

        Assert.Equal(
            SubscriptionPlan.Premium,
            subscription.Plan);

        Assert.Equal(
            BillingInterval.Yearly,
            subscription.BillingInterval);

        Assert.Equal(
            BillingSubscriptionStatus.PastDue,
            subscription.Status);

        Assert.Equal(
            "price_premium_yearly",
            subscription.ProviderPriceId);

        Assert.Equal(
            periodStart,
            subscription.CurrentPeriodStart);

        Assert.Equal(
            periodEnd,
            subscription.CurrentPeriodEnd);

        Assert.True(
            subscription.CancelAtPeriodEnd);

        Assert.Equal(
            "cus_123",
            subscription.ProviderCustomerId);

        Assert.Equal(
            "sub_123",
            subscription.ProviderSubscriptionId);
    }

    [Fact]
    public void ApplyProviderEvent_ShouldApplyNewerEvent()
    {
        var subscription =
            CreateSubscription();

        var providerEventCreatedAt =
            new DateTimeOffset(
                2026,
                9,
                10,
                12,
                0,
                0,
                TimeSpan.Zero);

        var periodStart =
            providerEventCreatedAt;

        var periodEnd =
            periodStart.AddYears(1);

        var applied =
            subscription.ApplyProviderEvent(
                providerEventCreatedAt,
                SubscriptionPlan.Premium,
                BillingInterval.Yearly,
                BillingSubscriptionStatus.Active,
                "price_premium_yearly",
                periodStart,
                periodEnd,
                false);

        Assert.True(
            applied);

        Assert.Equal(
            SubscriptionPlan.Premium,
            subscription.Plan);

        Assert.Equal(
            BillingInterval.Yearly,
            subscription.BillingInterval);

        Assert.Equal(
            BillingSubscriptionStatus.Active,
            subscription.Status);

        Assert.Equal(
            "price_premium_yearly",
            subscription.ProviderPriceId);

        Assert.Equal(
            periodStart,
            subscription.CurrentPeriodStart);

        Assert.Equal(
            periodEnd,
            subscription.CurrentPeriodEnd);

        Assert.Equal(
            providerEventCreatedAt,
            subscription.LastProviderEventCreatedAt);

        Assert.False(
            subscription.CancelAtPeriodEnd);
    }

    [Fact]
    public void ApplyProviderEvent_ShouldIgnoreOlderEvent()
    {
        var subscription =
            CreateSubscription();

        var newerEventCreatedAt =
            new DateTimeOffset(
                2026,
                9,
                10,
                12,
                0,
                0,
                TimeSpan.Zero);

        var newerPeriodStart =
            newerEventCreatedAt;

        var newerPeriodEnd =
            newerPeriodStart.AddYears(1);

        subscription.ApplyProviderEvent(
            newerEventCreatedAt,
            SubscriptionPlan.Premium,
            BillingInterval.Yearly,
            BillingSubscriptionStatus.Active,
            "price_premium_yearly",
            newerPeriodStart,
            newerPeriodEnd,
            false);

        var olderEventCreatedAt =
            newerEventCreatedAt.AddMinutes(-1);

        var olderPeriodStart =
            olderEventCreatedAt;

        var olderPeriodEnd =
            olderPeriodStart.AddMonths(1);

        var applied =
            subscription.ApplyProviderEvent(
                olderEventCreatedAt,
                SubscriptionPlan.Plus,
                BillingInterval.Monthly,
                BillingSubscriptionStatus.Canceled,
                "price_plus_monthly",
                olderPeriodStart,
                olderPeriodEnd,
                true);

        Assert.False(
            applied);

        Assert.Equal(
            SubscriptionPlan.Premium,
            subscription.Plan);

        Assert.Equal(
            BillingInterval.Yearly,
            subscription.BillingInterval);

        Assert.Equal(
            BillingSubscriptionStatus.Active,
            subscription.Status);

        Assert.Equal(
            "price_premium_yearly",
            subscription.ProviderPriceId);

        Assert.Equal(
            newerPeriodStart,
            subscription.CurrentPeriodStart);

        Assert.Equal(
            newerPeriodEnd,
            subscription.CurrentPeriodEnd);

        Assert.Equal(
            newerEventCreatedAt,
            subscription.LastProviderEventCreatedAt);

        Assert.False(
            subscription.CancelAtPeriodEnd);
    }

    [Fact]
    public void ScheduleCancellation_ShouldKeepSubscriptionActiveUntilPeriodEnd()
    {
        var subscription =
            CreateSubscription();

        var periodEnd =
            subscription.CurrentPeriodEnd;

        subscription.ScheduleCancellation();

        Assert.Equal(
            BillingSubscriptionStatus.Active,
            subscription.Status);

        Assert.True(
            subscription.CancelAtPeriodEnd);

        Assert.Equal(
            periodEnd,
            subscription.CurrentPeriodEnd);
    }

    [Fact]
    public void ScheduleCancellation_WhenSubscriptionHasEnded_ShouldThrow()
    {
        var subscription =
            CreateSubscription();

        subscription.Synchronize(
            SubscriptionPlan.Plus,
            BillingInterval.Monthly,
            BillingSubscriptionStatus.Canceled,
            "price_plus_monthly",
            subscription.CurrentPeriodStart,
            subscription.CurrentPeriodEnd,
            true);

        Assert.Throws<InvalidOperationException>(
            subscription.ScheduleCancellation);
    }

    [Fact]
    public void GrantsPaidAccessAt_ShouldReturnTrueForActiveSubscriptionDuringPeriod()
    {
        var subscription =
            CreateSubscription();

        var date =
            subscription.CurrentPeriodStart.AddDays(1);

        var result =
            subscription.GrantsPaidAccessAt(
                date);

        Assert.True(
            result);
    }

    [Fact]
    public void GrantsPaidAccessAt_ShouldReturnTrueForTrialingSubscriptionDuringPeriod()
    {
        var subscription =
            CreateSubscription();

        subscription.Synchronize(
            SubscriptionPlan.Plus,
            BillingInterval.Monthly,
            BillingSubscriptionStatus.Trialing,
            "price_plus_monthly",
            subscription.CurrentPeriodStart,
            subscription.CurrentPeriodEnd,
            false);

        var date =
            subscription.CurrentPeriodStart.AddDays(1);

        var result =
            subscription.GrantsPaidAccessAt(
                date);

        Assert.True(
            result);
    }

    [Theory]
    [InlineData(BillingSubscriptionStatus.Incomplete)]
    [InlineData(BillingSubscriptionStatus.IncompleteExpired)]
    [InlineData(BillingSubscriptionStatus.PastDue)]
    [InlineData(BillingSubscriptionStatus.Canceled)]
    [InlineData(BillingSubscriptionStatus.Unpaid)]
    [InlineData(BillingSubscriptionStatus.Paused)]
    public void GrantsPaidAccessAt_ShouldReturnFalseForStatusWithoutAccess(
        BillingSubscriptionStatus status)
    {
        var subscription =
            CreateSubscription();

        subscription.Synchronize(
            SubscriptionPlan.Plus,
            BillingInterval.Monthly,
            status,
            "price_plus_monthly",
            subscription.CurrentPeriodStart,
            subscription.CurrentPeriodEnd,
            false);

        var date =
            subscription.CurrentPeriodStart.AddDays(1);

        var result =
            subscription.GrantsPaidAccessAt(
                date);

        Assert.False(
            result);
    }

    [Fact]
    public void GrantsPaidAccessAt_ShouldReturnFalseBeforePeriodStart()
    {
        var subscription =
            CreateSubscription();

        var result =
            subscription.GrantsPaidAccessAt(
                subscription.CurrentPeriodStart.AddTicks(-1));

        Assert.False(
            result);
    }

    [Fact]
    public void GrantsPaidAccessAt_ShouldReturnFalseAtPeriodEnd()
    {
        var subscription =
            CreateSubscription();

        var result =
            subscription.GrantsPaidAccessAt(
                subscription.CurrentPeriodEnd);

        Assert.False(
            result);
    }

    [Fact]
    public void GrantsPaidAccessAt_WhenCancellationIsScheduled_ShouldReturnTrueUntilPeriodEnd()
    {
        var subscription =
            CreateSubscription();

        subscription.ScheduleCancellation();

        var date =
            subscription.CurrentPeriodEnd.AddTicks(-1);

        var result =
            subscription.GrantsPaidAccessAt(
                date);

        Assert.True(
            result);
    }

    [Fact]
    public void GrantsPaidAccessAt_WithMissingDate_ShouldThrow()
    {
        var subscription =
            CreateSubscription();

        var exception =
            Assert.Throws<ArgumentException>(() =>
                subscription.GrantsPaidAccessAt(
                    default));

        Assert.Equal(
            "date",
            exception.ParamName);
    }

    [Fact]
    public void PreventsAccountDeletion_WithoutProviderSubscription_ShouldReturnFalse()
    {
        var subscription =
            CreateSubscription();

        var result =
            subscription.PreventsAccountDeletion();

        Assert.False(
            result);
    }

    [Theory]
    [InlineData(BillingSubscriptionStatus.Active)]
    [InlineData(BillingSubscriptionStatus.Trialing)]
    [InlineData(BillingSubscriptionStatus.PastDue)]
    [InlineData(BillingSubscriptionStatus.Unpaid)]
    [InlineData(BillingSubscriptionStatus.Paused)]
    public void PreventsAccountDeletion_WithExistingProviderSubscription_ShouldReturnTrue(
        BillingSubscriptionStatus status)
    {
        var subscription =
            CreateLinkedSubscription();

        subscription.Synchronize(
            SubscriptionPlan.Plus,
            BillingInterval.Monthly,
            status,
            "price_plus_monthly",
            subscription.CurrentPeriodStart,
            subscription.CurrentPeriodEnd,
            false);

        var result =
            subscription.PreventsAccountDeletion();

        Assert.True(
            result);
    }

    [Fact]
    public void PreventsAccountDeletion_WhenCancellationIsScheduled_ShouldReturnTrue()
    {
        var subscription =
            CreateLinkedSubscription();

        subscription.ScheduleCancellation();

        var result =
            subscription.PreventsAccountDeletion();

        Assert.True(
            result);
    }

    [Theory]
    [InlineData(BillingSubscriptionStatus.Canceled)]
    [InlineData(BillingSubscriptionStatus.IncompleteExpired)]
    public void PreventsAccountDeletion_WhenProviderSubscriptionHasEnded_ShouldReturnFalse(
        BillingSubscriptionStatus status)
    {
        var subscription =
            CreateLinkedSubscription();

        subscription.Synchronize(
            SubscriptionPlan.Plus,
            BillingInterval.Monthly,
            status,
            "price_plus_monthly",
            subscription.CurrentPeriodStart,
            subscription.CurrentPeriodEnd,
            true);

        var result =
            subscription.PreventsAccountDeletion();

        Assert.False(
            result);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void LinkToPaymentProvider_WithInvalidCustomerId_ShouldThrow(
        string? customerId)
    {
        var subscription =
            CreateSubscription();

        var exception =
            Assert.ThrowsAny<ArgumentException>(() =>
                subscription.LinkToPaymentProvider(
                    customerId!,
                    "sub_123",
                    "price_123"));

        Assert.Equal(
            "customerId",
            exception.ParamName);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void LinkToPaymentProvider_WithInvalidSubscriptionId_ShouldThrow(
        string? subscriptionId)
    {
        var subscription =
            CreateSubscription();

        var exception =
            Assert.ThrowsAny<ArgumentException>(() =>
                subscription.LinkToPaymentProvider(
                    "cus_123",
                    subscriptionId!,
                    "price_123"));

        Assert.Equal(
            "subscriptionId",
            exception.ParamName);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void LinkToPaymentProvider_WithInvalidPriceId_ShouldThrow(
        string? priceId)
    {
        var subscription =
            CreateSubscription();

        var exception =
            Assert.ThrowsAny<ArgumentException>(() =>
                subscription.LinkToPaymentProvider(
                    "cus_123",
                    "sub_123",
                    priceId!));

        Assert.Equal(
            "priceId",
            exception.ParamName);
    }

    [Fact]
    public void Create_WhenPeriodEndIsNotAfterStart_ShouldThrow()
    {
        var periodStart = new DateTimeOffset(
            2026, 8, 9, 0, 0, 0, TimeSpan.Zero);

        var exception =
            Assert.Throws<ArgumentException>(() =>
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

    [Fact]
    public void Create_WithFreePlan_ShouldThrow()
    {
        var periodStart = new DateTimeOffset(
            2026, 8, 9, 0, 0, 0, TimeSpan.Zero);

        var periodEnd =
            periodStart.AddMonths(1);

        var exception =
            Assert.Throws<ArgumentException>(() =>
                new BillingSubscription(
                    Guid.NewGuid(),
                    Guid.NewGuid(),
                    SubscriptionPlan.Free,
                    BillingInterval.Monthly,
                    periodStart,
                    periodEnd));

        Assert.Equal(
            "plan",
            exception.ParamName);
    }

    private static BillingSubscription CreateSubscription()
    {
        var periodStart = new DateTimeOffset(
            2026, 8, 9, 0, 0, 0, TimeSpan.Zero);

        return new BillingSubscription(
            Guid.NewGuid(),
            Guid.NewGuid(),
            SubscriptionPlan.Plus,
            BillingInterval.Monthly,
            periodStart,
            periodStart.AddMonths(1));
    }

    private static BillingSubscription CreateLinkedSubscription()
    {
        var subscription =
            CreateSubscription();

        subscription.LinkToPaymentProvider(
            "cus_123",
            "sub_123",
            "price_plus_monthly");

        return subscription;
    }
}
