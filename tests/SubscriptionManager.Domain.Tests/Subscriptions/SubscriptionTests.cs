using SubscriptionManager.Domain.Subscriptions;

namespace SubscriptionManager.Domain.Tests.Subscriptions;

public class SubscriptionTests
{
    [Fact]
    public void Constructor_ShouldCreateSubscription_WhenArgumentsAreValid()
    {
        var startDate = new DateOnly(2026, 1, 1);

        var subscription = new Subscription(
            Guid.NewGuid(),
            "Netflix",
            49m,
            "PLN",
            BillingPeriod.Monthly,
            startDate);

        Assert.Equal("Netflix", subscription.Name);
        Assert.Equal(49m, subscription.Amount);
        Assert.Equal("PLN", subscription.Currency);
        Assert.Equal(BillingPeriod.Monthly, subscription.BillingPeriod);
        Assert.Equal(startDate, subscription.StartDate);
        Assert.Null(subscription.EndDate);
        Assert.True(subscription.IsActive);
    }

    [Fact]
    public void Constructor_ShouldTrimName_WhenNameContainsLeadingOrTrailingWhitespace()
    {
        var subscription = CreateSubscription(name: "  Netflix  ");

        Assert.Equal("Netflix", subscription.Name);
    }

    [Fact]
    public void Constructor_ShouldNormalizeCurrency_WhenCurrencyContainsLowercaseLetters()
    {
        var subscription = CreateSubscription(currency: "pln");

        Assert.Equal("PLN", subscription.Currency);
    }

    [Fact]
    public void Constructor_ShouldThrowArgumentException_WhenIdentifierIsEmpty()
    {
        Assert.Throws<ArgumentException>(() =>
            new Subscription(
                Guid.Empty,
                "Netflix",
                49m,
                "PLN",
                BillingPeriod.Monthly,
                new DateOnly(2026, 1, 1)));
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    public void Constructor_ShouldThrowArgumentException_WhenNameIsInvalid(
        string name)
    {
        Assert.Throws<ArgumentException>(() =>
            CreateSubscription(name: name));
    }

    [Fact]
    public void Constructor_ShouldThrowArgumentException_WhenNameExceedsMaximumLength()
    {
        var name = new string(
            'a',
            Subscription.MaxNameLength + 1);

        Assert.Throws<ArgumentException>(() =>
            CreateSubscription(name: name));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-99.99)]
    public void Constructor_ShouldThrowArgumentOutOfRangeException_WhenAmountIsNotGreaterThanZero(
        decimal amount)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            CreateSubscription(amount: amount));
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("PL")]
    [InlineData("PLNN")]
    [InlineData("P1N")]
    [InlineData("12!")]
    public void Constructor_ShouldThrowArgumentException_WhenCurrencyIsInvalid(
        string currency)
    {
        Assert.Throws<ArgumentException>(() =>
            CreateSubscription(currency: currency));
    }

    [Fact]
    public void Constructor_ShouldThrowArgumentOutOfRangeException_WhenBillingPeriodIsInvalid()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            CreateSubscription(billingPeriod: (BillingPeriod)999));
    }

    [Fact]
    public void Update_ShouldUpdateSubscription_WhenArgumentsAreValid()
    {
        var subscription = CreateSubscription();

        subscription.Update(
            "Spotify",
            65m,
            "EUR",
            BillingPeriod.Yearly);

        Assert.Equal("Spotify", subscription.Name);
        Assert.Equal(65m, subscription.Amount);
        Assert.Equal("EUR", subscription.Currency);
        Assert.Equal(BillingPeriod.Yearly, subscription.BillingPeriod);
    }

    [Fact]
    public void Update_ShouldNormalizeArguments_WhenArgumentsAreValid()
    {
        var subscription = CreateSubscription();

        subscription.Update(
            "  Spotify  ",
            65m,
            "eur",
            BillingPeriod.Yearly);

        Assert.Equal("Spotify", subscription.Name);
        Assert.Equal("EUR", subscription.Currency);
    }

    [Fact]
    public void Update_ShouldThrowArgumentException_WhenNameExceedsMaximumLength()
    {
        var subscription = CreateSubscription();
        var name = new string(
            'a',
            Subscription.MaxNameLength + 1);

        Assert.Throws<ArgumentException>(() =>
            subscription.Update(
                name,
                65m,
                "EUR",
                BillingPeriod.Yearly));
    }

    [Fact]
    public void End_ShouldMarkSubscriptionAsInactive_WhenEndDateIsValid()
    {
        var subscription = CreateSubscription();
        var endDate = new DateOnly(2026, 2, 1);

        subscription.End(endDate);

        Assert.Equal(endDate, subscription.EndDate);
        Assert.False(subscription.IsActive);
    }

    [Fact]
    public void End_ShouldThrowInvalidOperationException_WhenSubscriptionHasAlreadyEnded()
    {
        var subscription = CreateSubscription();

        subscription.End(new DateOnly(2026, 2, 1));

        Assert.Throws<InvalidOperationException>(() =>
            subscription.End(new DateOnly(2026, 3, 1)));
    }

    [Fact]
    public void End_ShouldThrowArgumentOutOfRangeException_WhenEndDateIsEarlierThanStartDate()
    {
        var subscription = CreateSubscription();

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            subscription.End(new DateOnly(2025, 12, 31)));
    }

    [Theory]
    [InlineData(BillingPeriod.Monthly, 49, 49)]
    [InlineData(BillingPeriod.Quarterly, 147, 49)]
    [InlineData(BillingPeriod.SemiAnnual, 294, 49)]
    [InlineData(BillingPeriod.Yearly, 588, 49)]
    public void MonthlyEquivalentAmount_ShouldReturnMonthlyAmount_ForSupportedBillingPeriods(
        BillingPeriod billingPeriod,
        decimal amount,
        decimal expectedMonthlyAmount)
    {
        var subscription = CreateSubscription(
            amount: amount,
            billingPeriod: billingPeriod);

        Assert.Equal(
            expectedMonthlyAmount,
            subscription.MonthlyEquivalentAmount);
    }

    [Theory]
    [InlineData(BillingPeriod.Monthly, 49, 588)]
    [InlineData(BillingPeriod.Quarterly, 147, 588)]
    [InlineData(BillingPeriod.SemiAnnual, 294, 588)]
    [InlineData(BillingPeriod.Yearly, 588, 588)]
    public void YearlyEquivalentAmount_ShouldReturnYearlyAmount_ForSupportedBillingPeriods(
        BillingPeriod billingPeriod,
        decimal amount,
        decimal expectedYearlyAmount)
    {
        var subscription = CreateSubscription(
            amount: amount,
            billingPeriod: billingPeriod);

        Assert.Equal(
            expectedYearlyAmount,
            subscription.YearlyEquivalentAmount);
    }

    private static Subscription CreateSubscription(
        string name = "Netflix",
        decimal amount = 49m,
        string currency = "PLN",
        BillingPeriod billingPeriod = BillingPeriod.Monthly)
    {
        return new Subscription(
            Guid.NewGuid(),
            name,
            amount,
            currency,
            billingPeriod,
            new DateOnly(2026, 1, 1));
    }
}
