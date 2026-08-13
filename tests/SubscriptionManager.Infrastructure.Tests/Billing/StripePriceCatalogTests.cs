using Microsoft.Extensions.Options;
using SubscriptionManager.Domain.Billing;
using SubscriptionManager.Infrastructure.Billing.Stripe;

namespace SubscriptionManager.Infrastructure.Tests.Billing;

public sealed class StripePriceCatalogTests
{
    private const string PlusMonthlyPriceId =
        "price_plus_monthly";

    private const string PlusYearlyPriceId =
        "price_plus_yearly";

    private const string PremiumMonthlyPriceId =
        "price_premium_monthly";

    private const string PremiumYearlyPriceId =
        "price_premium_yearly";

    private readonly StripePriceCatalog _catalog =
        CreateCatalog();

    [Theory]
    [InlineData(
        SubscriptionPlan.Plus,
        BillingInterval.Monthly,
        PlusMonthlyPriceId)]
    [InlineData(
        SubscriptionPlan.Plus,
        BillingInterval.Yearly,
        PlusYearlyPriceId)]
    [InlineData(
        SubscriptionPlan.Premium,
        BillingInterval.Monthly,
        PremiumMonthlyPriceId)]
    [InlineData(
        SubscriptionPlan.Premium,
        BillingInterval.Yearly,
        PremiumYearlyPriceId)]
    public void GetPriceId_ShouldReturnConfiguredPrice(
        SubscriptionPlan plan,
        BillingInterval billingInterval,
        string expectedPriceId)
    {
        var result =
            _catalog.GetPriceId(
                plan,
                billingInterval);

        Assert.Equal(
            expectedPriceId,
            result);
    }

    [Theory]
    [InlineData(
        PlusMonthlyPriceId,
        SubscriptionPlan.Plus,
        BillingInterval.Monthly)]
    [InlineData(
        PlusYearlyPriceId,
        SubscriptionPlan.Plus,
        BillingInterval.Yearly)]
    [InlineData(
        PremiumMonthlyPriceId,
        SubscriptionPlan.Premium,
        BillingInterval.Monthly)]
    [InlineData(
        PremiumYearlyPriceId,
        SubscriptionPlan.Premium,
        BillingInterval.Yearly)]
    public void TryGetPlan_ShouldReturnPlanForConfiguredPrice(
        string priceId,
        SubscriptionPlan expectedPlan,
        BillingInterval expectedBillingInterval)
    {
        var result =
            _catalog.TryGetPlan(
                priceId,
                out var plan,
                out var billingInterval);

        Assert.True(
            result);

        Assert.Equal(
            expectedPlan,
            plan);

        Assert.Equal(
            expectedBillingInterval,
            billingInterval);
    }

    [Fact]
    public void TryGetPlan_ShouldReturnFalseForUnknownPrice()
    {
        var result =
            _catalog.TryGetPlan(
                "price_unknown",
                out var plan,
                out var billingInterval);

        Assert.False(
            result);

        Assert.Equal(
            default,
            plan);

        Assert.Equal(
            default,
            billingInterval);
    }

    [Fact]
    public void GetPriceId_WithUnsupportedPlan_ShouldThrow()
    {
        var exception =
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                _catalog.GetPriceId(
                    SubscriptionPlan.Free,
                    BillingInterval.Monthly));

        Assert.Equal(
            "plan",
            exception.ParamName);
    }

    [Fact]
    public void GetPriceId_WithUnsupportedBillingInterval_ShouldThrow()
    {
        var exception =
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                _catalog.GetPriceId(
                    SubscriptionPlan.Plus,
                    (BillingInterval)999));

        Assert.Equal(
            "plan",
            exception.ParamName);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void TryGetPlan_WithMissingPriceId_ShouldThrow(
        string? priceId)
    {
        var exception =
            Assert.ThrowsAny<ArgumentException>(() =>
                _catalog.TryGetPlan(
                    priceId!,
                    out _,
                    out _));

        Assert.Equal(
            "priceId",
            exception.ParamName);
    }

    private static StripePriceCatalog CreateCatalog()
    {
        var options =
            Options.Create(
                new StripeOptions
                {
                    PlusMonthlyPriceId =
                        PlusMonthlyPriceId,

                    PlusYearlyPriceId =
                        PlusYearlyPriceId,

                    PremiumMonthlyPriceId =
                        PremiumMonthlyPriceId,

                    PremiumYearlyPriceId =
                        PremiumYearlyPriceId
                });

        return new StripePriceCatalog(
            options);
    }
}
