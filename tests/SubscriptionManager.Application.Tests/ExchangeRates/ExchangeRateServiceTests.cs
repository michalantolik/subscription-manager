using Moq;
using SubscriptionManager.Application.ExchangeRates;
using SubscriptionManager.Domain.ExchangeRates;
using SubscriptionManager.Domain.Subscriptions;

namespace SubscriptionManager.Application.Tests.ExchangeRates;

public sealed class ExchangeRateServiceTests
{
    private static readonly DateTimeOffset CurrentTime =
        new(
            2026,
            8,
            2,
            12,
            0,
            0,
            TimeSpan.Zero);

    private static readonly DateOnly EffectiveDate =
        new(2026, 7, 31);

    [Fact]
    public async Task GetCurrentAsync_ShouldUseStoredRates_WhenRatesWereCheckedToday()
    {
        var storedRates =
            CreateStoredRates(CurrentTime);

        var repository =
            new Mock<IExchangeRateRepository>();

        var provider =
            new Mock<IExchangeRateProvider>();

        repository
            .Setup(currentRepository =>
                currentRepository.GetAllAsync(
                    It.IsAny<CancellationToken>()))
            .ReturnsAsync(storedRates);

        var service =
            CreateService(
                repository,
                provider);

        var result =
            await service.GetCurrentAsync();

        Assert.Equal(
            EffectiveDate,
            result.EffectiveDate);

        Assert.Equal(
            9,
            result.RatesToPln.Count);

        Assert.Equal(
            1m,
            result.RatesToPln[Currency.PLN]);

        Assert.Equal(
            20m / 3m,
            result.Convert(
                10m,
                Currency.EUR,
                Currency.USD));

        provider.Verify(
            currentProvider =>
                currentProvider.GetLatestAsync(
                    It.IsAny<CancellationToken>()),
            Times.Never);

        repository.Verify(
            currentRepository =>
                currentRepository.SaveChangesAsync(
                    It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task GetCurrentAsync_ShouldRefreshStoredRates_WhenRatesWereNotCheckedToday()
    {
        var previousCheckedAt =
            CurrentTime.AddDays(-1);

        var storedRates =
            CreateStoredRates(previousCheckedAt);

        var snapshot =
            CreateSnapshot(
                new DateOnly(2026, 8, 1),
                rateOffset: 10m);

        var repository =
            new Mock<IExchangeRateRepository>();

        var provider =
            new Mock<IExchangeRateProvider>();

        repository
            .Setup(currentRepository =>
                currentRepository.GetAllAsync(
                    It.IsAny<CancellationToken>()))
            .ReturnsAsync(storedRates);

        provider
            .Setup(currentProvider =>
                currentProvider.GetLatestAsync(
                    It.IsAny<CancellationToken>()))
            .ReturnsAsync(snapshot);

        var service =
            CreateService(
                repository,
                provider);

        var result =
            await service.GetCurrentAsync();

        Assert.Equal(
            snapshot.EffectiveDate,
            result.EffectiveDate);

        foreach (var rate in storedRates)
        {
            Assert.Equal(
                GetRate(rate.Currency, 10m),
                rate.RateToPln);

            Assert.Equal(
                snapshot.EffectiveDate,
                rate.EffectiveDate);

            Assert.Equal(
                CurrentTime,
                rate.LastCheckedAt);
        }

        repository.Verify(
            currentRepository =>
                currentRepository.AddRangeAsync(
                    It.IsAny<IEnumerable<ExchangeRate>>(),
                    It.IsAny<CancellationToken>()),
            Times.Never);

        repository.Verify(
            currentRepository =>
                currentRepository.SaveChangesAsync(
                    It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetCurrentAsync_ShouldStoreRates_WhenNoRatesHaveBeenStored()
    {
        var snapshot =
            CreateSnapshot(
                new DateOnly(2026, 8, 1),
                rateOffset: 10m);

        ExchangeRate[]? addedRates = null;

        var repository =
            new Mock<IExchangeRateRepository>();

        var provider =
            new Mock<IExchangeRateProvider>();

        repository
            .Setup(currentRepository =>
                currentRepository.GetAllAsync(
                    It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                Array.Empty<ExchangeRate>());

        repository
            .Setup(currentRepository =>
                currentRepository.AddRangeAsync(
                    It.IsAny<IEnumerable<ExchangeRate>>(),
                    It.IsAny<CancellationToken>()))
            .Callback<
                IEnumerable<ExchangeRate>,
                CancellationToken>(
                (rates, _) =>
                    addedRates = rates.ToArray())
            .Returns(Task.CompletedTask);

        provider
            .Setup(currentProvider =>
                currentProvider.GetLatestAsync(
                    It.IsAny<CancellationToken>()))
            .ReturnsAsync(snapshot);

        var service =
            CreateService(
                repository,
                provider);

        var result =
            await service.GetCurrentAsync();

        Assert.NotNull(addedRates);

        Assert.Equal(
            8,
            addedRates.Length);

        Assert.DoesNotContain(
            addedRates,
            rate =>
                rate.Currency == Currency.PLN);

        Assert.All(
            addedRates,
            rate =>
            {
                Assert.Equal(
                    snapshot.EffectiveDate,
                    rate.EffectiveDate);

                Assert.Equal(
                    CurrentTime,
                    rate.LastCheckedAt);
            });

        Assert.Equal(
            9,
            result.RatesToPln.Count);

        Assert.Equal(
            1m,
            result.RatesToPln[Currency.PLN]);

        repository.Verify(
            currentRepository =>
                currentRepository.SaveChangesAsync(
                    It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetCurrentAsync_ShouldUseStoredRates_WhenProviderFails()
    {
        var previousCheckedAt =
            CurrentTime.AddDays(-1);

        var storedRates =
            CreateStoredRates(previousCheckedAt);

        var repository =
            new Mock<IExchangeRateRepository>();

        var provider =
            new Mock<IExchangeRateProvider>();

        repository
            .Setup(currentRepository =>
                currentRepository.GetAllAsync(
                    It.IsAny<CancellationToken>()))
            .ReturnsAsync(storedRates);

        provider
            .Setup(currentProvider =>
                currentProvider.GetLatestAsync(
                    It.IsAny<CancellationToken>()))
            .ThrowsAsync(
                new HttpRequestException(
                    "NBP is unavailable."));

        var service =
            CreateService(
                repository,
                provider);

        var result =
            await service.GetCurrentAsync();

        Assert.Equal(
            EffectiveDate,
            result.EffectiveDate);

        Assert.All(
            storedRates,
            rate =>
                Assert.Equal(
                    CurrentTime,
                    rate.LastCheckedAt));

        repository.Verify(
            currentRepository =>
                currentRepository.SaveChangesAsync(
                    It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetCurrentAsync_ShouldThrow_WhenProviderFailsAndStoredRatesAreIncomplete()
    {
        var storedRates =
            CreateStoredRates(
                    CurrentTime.AddDays(-1))
                .Where(rate =>
                    rate.Currency != Currency.EUR)
                .ToArray();

        var repository =
            new Mock<IExchangeRateRepository>();

        var provider =
            new Mock<IExchangeRateProvider>();

        repository
            .Setup(currentRepository =>
                currentRepository.GetAllAsync(
                    It.IsAny<CancellationToken>()))
            .ReturnsAsync(storedRates);

        provider
            .Setup(currentProvider =>
                currentProvider.GetLatestAsync(
                    It.IsAny<CancellationToken>()))
            .ThrowsAsync(
                new HttpRequestException(
                    "NBP is unavailable."));

        var service =
            CreateService(
                repository,
                provider);

        var exception =
            await Assert.ThrowsAsync<
                ExchangeRatesUnavailableException>(
                () => service.GetCurrentAsync());

        Assert.IsType<HttpRequestException>(
            exception.InnerException);

        repository.Verify(
            currentRepository =>
                currentRepository.SaveChangesAsync(
                    It.IsAny<CancellationToken>()),
            Times.Never);
    }

    private static ExchangeRateService CreateService(
        Mock<IExchangeRateRepository> repository,
        Mock<IExchangeRateProvider> provider)
    {
        return new ExchangeRateService(
            repository.Object,
            provider.Object,
            new FixedTimeProvider(CurrentTime));
    }

    private static ExchangeRate[] CreateStoredRates(
        DateTimeOffset checkedAt)
    {
        return GetForeignCurrencies()
            .Select(currency =>
                new ExchangeRate(
                    currency,
                    GetRate(currency),
                    EffectiveDate,
                    checkedAt))
            .ToArray();
    }

    private static ExchangeRateSnapshot CreateSnapshot(
        DateOnly effectiveDate,
        decimal rateOffset)
    {
        var quotes =
            GetForeignCurrencies()
                .Select(currency =>
                    new ExchangeRateQuote(
                        currency,
                        GetRate(
                            currency,
                            rateOffset)))
                .ToArray();

        return new ExchangeRateSnapshot(
            effectiveDate,
            quotes);
    }

    private static Currency[] GetForeignCurrencies()
    {
        return Enum.GetValues<Currency>()
            .Where(currency =>
                currency != Currency.PLN)
            .ToArray();
    }

    private static decimal GetRate(
        Currency currency,
        decimal offset = 0m)
    {
        return (int)currency +
               offset;
    }

    private sealed class FixedTimeProvider(
        DateTimeOffset utcNow)
        : TimeProvider
    {
        public override DateTimeOffset GetUtcNow()
        {
            return utcNow;
        }
    }
}
