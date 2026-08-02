using SubscriptionManager.Domain.ExchangeRates;
using SubscriptionManager.Domain.Subscriptions;

namespace SubscriptionManager.Domain.Tests.ExchangeRates;

public sealed class ExchangeRateTests
{
    [Fact]
    public void Constructor_ShouldCreateExchangeRate_WhenValuesAreValid()
    {
        var effectiveDate =
            new DateOnly(2026, 8, 3);

        var checkedAt =
            new DateTimeOffset(
                2026,
                8,
                3,
                12,
                0,
                0,
                TimeSpan.Zero);

        var exchangeRate =
            new ExchangeRate(
                Currency.EUR,
                4.25m,
                effectiveDate,
                checkedAt);

        Assert.Equal(
            Currency.EUR,
            exchangeRate.Currency);

        Assert.Equal(
            4.25m,
            exchangeRate.RateToPln);

        Assert.Equal(
            effectiveDate,
            exchangeRate.EffectiveDate);

        Assert.Equal(
            checkedAt,
            exchangeRate.LastCheckedAt);
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenCurrencyIsPln()
    {
        Assert.Throws<ArgumentException>(() =>
            new ExchangeRate(
                Currency.PLN,
                1m,
                new DateOnly(2026, 8, 3),
                DateTimeOffset.UtcNow));
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenCurrencyIsNotSupported()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new ExchangeRate(
                (Currency)999,
                1m,
                new DateOnly(2026, 8, 3),
                DateTimeOffset.UtcNow));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Constructor_ShouldThrow_WhenRateIsNotPositive(
        decimal rateToPln)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new ExchangeRate(
                Currency.EUR,
                rateToPln,
                new DateOnly(2026, 8, 3),
                DateTimeOffset.UtcNow));
    }

    [Fact]
    public void Update_ShouldReplaceRateAndDates_WhenValuesAreValid()
    {
        var exchangeRate =
            new ExchangeRate(
                Currency.EUR,
                4.20m,
                new DateOnly(2026, 8, 1),
                new DateTimeOffset(
                    2026,
                    8,
                    1,
                    12,
                    0,
                    0,
                    TimeSpan.Zero));

        var effectiveDate =
            new DateOnly(2026, 8, 3);

        var checkedAt =
            new DateTimeOffset(
                2026,
                8,
                3,
                12,
                0,
                0,
                TimeSpan.Zero);

        exchangeRate.Update(
            4.25m,
            effectiveDate,
            checkedAt);

        Assert.Equal(
            4.25m,
            exchangeRate.RateToPln);

        Assert.Equal(
            effectiveDate,
            exchangeRate.EffectiveDate);

        Assert.Equal(
            checkedAt,
            exchangeRate.LastCheckedAt);
    }

    [Fact]
    public void MarkAsChecked_ShouldUpdateOnlyLastCheckedAt()
    {
        var effectiveDate =
            new DateOnly(2026, 8, 1);

        var exchangeRate =
            new ExchangeRate(
                Currency.EUR,
                4.20m,
                effectiveDate,
                new DateTimeOffset(
                    2026,
                    8,
                    1,
                    12,
                    0,
                    0,
                    TimeSpan.Zero));

        var checkedAt =
            new DateTimeOffset(
                2026,
                8,
                3,
                12,
                0,
                0,
                TimeSpan.Zero);

        exchangeRate.MarkAsChecked(
            checkedAt);

        Assert.Equal(
            4.20m,
            exchangeRate.RateToPln);

        Assert.Equal(
            effectiveDate,
            exchangeRate.EffectiveDate);

        Assert.Equal(
            checkedAt,
            exchangeRate.LastCheckedAt);
    }

    [Fact]
    public void MarkAsChecked_ShouldThrow_WhenCheckDateIsMissing()
    {
        var exchangeRate =
            new ExchangeRate(
                Currency.EUR,
                4.20m,
                new DateOnly(2026, 8, 1),
                DateTimeOffset.UtcNow);

        Assert.Throws<ArgumentException>(() =>
            exchangeRate.MarkAsChecked(default));
    }
}
