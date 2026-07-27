using SubscriptionManager.Domain.DigitalServices;
using SubscriptionManager.Domain.Subscriptions;

namespace SubscriptionManager.Domain.Tests.Subscriptions;

public class SubscriptionTests
{
    [Fact]
    public void Constructor_ShouldCreateSubscription_WhenArgumentsAreValid()
    {
        var id = Guid.NewGuid();
        var ownerId = Guid.NewGuid();
        var startDate = new DateOnly(2026, 1, 1);

        var subscription = new Subscription(
            id,
            ownerId,
            "Netflix",
            49m,
            "PLN",
            BillingPeriod.Monthly,
            startDate);

        Assert.Equal(id, subscription.Id);
        Assert.Equal(ownerId, subscription.OwnerId);
        Assert.Null(subscription.DigitalServiceId);
        Assert.Equal("Netflix", subscription.Name);
        Assert.Null(subscription.Category);
        Assert.Null(subscription.CustomCategoryName);
        Assert.Null(subscription.IconKey);
        Assert.Null(subscription.ManagementUrl);
        Assert.Equal(49m, subscription.Amount);
        Assert.Equal("PLN", subscription.Currency);
        Assert.Equal(BillingPeriod.Monthly, subscription.BillingPeriod);
        Assert.Equal(startDate, subscription.StartDate);
        Assert.Null(subscription.EndDate);
        Assert.True(subscription.IsActive);
    }

    [Fact]
    public void AssignDigitalService_ShouldStoreServiceSnapshot()
    {
        var subscription = CreateSubscription(name: "Temporary name");
        var digitalServiceId = Guid.NewGuid();

        subscription.AssignDigitalService(
            digitalServiceId,
            DigitalServiceCategory.Video,
            null,
            "  netflix  ",
            "  https://www.netflix.com/account  ");

        Assert.Equal(
            digitalServiceId,
            subscription.DigitalServiceId);
        Assert.Equal("Temporary name", subscription.Name);
        Assert.Equal(
            DigitalServiceCategory.Video,
            subscription.Category);
        Assert.Null(subscription.CustomCategoryName);
        Assert.Equal("netflix", subscription.IconKey);
        Assert.Equal(
            "https://www.netflix.com/account",
            subscription.ManagementUrl);
    }

    [Fact]
    public void AssignDigitalService_ShouldStoreCustomCategoryName_WhenCategoryIsOther()
    {
        var subscription = CreateSubscription();
        var digitalServiceId = Guid.NewGuid();

        subscription.AssignDigitalService(
            digitalServiceId,
            DigitalServiceCategory.Other,
            "  Streaming  ",
            null,
            null);

        Assert.Equal(
            DigitalServiceCategory.Other,
            subscription.Category);
        Assert.Equal(
            "Streaming",
            subscription.CustomCategoryName);
    }

    [Fact]
    public void AssignDigitalService_ShouldThrow_WhenDigitalServiceIdentifierIsEmpty()
    {
        var subscription = CreateSubscription();

        var exception = Assert.Throws<ArgumentException>(() =>
            subscription.AssignDigitalService(
                Guid.Empty,
                DigitalServiceCategory.Video,
                null,
                "netflix",
                "https://www.netflix.com/account"));

        Assert.Equal("digitalServiceId", exception.ParamName);
    }

    [Fact]
    public void AssignDigitalService_ShouldThrow_WhenCustomCategoryIsUsedWithKnownCategory()
    {
        var subscription = CreateSubscription();

        var exception = Assert.Throws<ArgumentException>(() =>
            subscription.AssignDigitalService(
                Guid.NewGuid(),
                DigitalServiceCategory.Video,
                "Streaming",
                "netflix",
                "https://www.netflix.com/account"));

        Assert.Equal("customCategoryName", exception.ParamName);
    }


    [Fact]
    public void ClearDigitalService_ShouldRemoveServiceSnapshot()
    {
        var subscription = CreateSubscription();

        subscription.AssignDigitalService(
            Guid.NewGuid(),
            DigitalServiceCategory.Video,
            null,
            "netflix",
            "https://www.netflix.com/account");

        subscription.ClearDigitalService();

        Assert.Null(subscription.DigitalServiceId);
        Assert.Null(subscription.Category);
        Assert.Null(subscription.CustomCategoryName);
        Assert.Null(subscription.IconKey);
        Assert.Null(subscription.ManagementUrl);
        Assert.Equal("Netflix", subscription.Name);
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
        var exception = Assert.Throws<ArgumentException>(() =>
            CreateSubscription(id: Guid.Empty));

        Assert.Equal("id", exception.ParamName);
    }

    [Fact]
    public void Constructor_ShouldThrowArgumentException_WhenOwnerIdentifierIsEmpty()
    {
        var exception = Assert.Throws<ArgumentException>(() =>
            CreateSubscription(ownerId: Guid.Empty));

        Assert.Equal("ownerId", exception.ParamName);
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
        Guid? id = null,
        Guid? ownerId = null,
        string name = "Netflix",
        decimal amount = 49m,
        string currency = "PLN",
        BillingPeriod billingPeriod = BillingPeriod.Monthly)
    {
        return new Subscription(
            id ?? Guid.NewGuid(),
            ownerId ?? Guid.NewGuid(),
            name,
            amount,
            currency,
            billingPeriod,
            new DateOnly(2026, 1, 1));
    }
}
