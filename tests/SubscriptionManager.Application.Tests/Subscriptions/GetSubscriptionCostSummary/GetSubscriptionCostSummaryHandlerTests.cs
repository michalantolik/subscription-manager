using Moq;
using SubscriptionManager.Application.Common.Identity;
using SubscriptionManager.Application.ExchangeRates;
using SubscriptionManager.Application.Subscriptions;
using SubscriptionManager.Application.Subscriptions.GetSubscriptionCostSummary;
using SubscriptionManager.Domain.DigitalServices;
using SubscriptionManager.Domain.Subscriptions;

namespace SubscriptionManager.Application.Tests.Subscriptions.GetSubscriptionCostSummary;

public sealed class GetSubscriptionCostSummaryHandlerTests
{
    [Fact]
    public async Task HandleAsync_ShouldCreateSummaryWithoutExchangeRates_WhenCurrenciesMatch()
    {
        var ownerId =
            Guid.NewGuid();

        var subscription =
            CreateSubscription(
                ownerId,
                "Netflix",
                120m,
                Currency.PLN,
                BillingPeriod.Yearly,
                DigitalServiceCategory.Video);

        var repository =
            new Mock<ISubscriptionRepository>();

        var identityService =
            new Mock<IIdentityService>();

        var exchangeRateService =
            new Mock<IExchangeRateService>();

        var currentUser =
            new Mock<ICurrentUser>();

        currentUser
            .SetupGet(user =>
                user.UserId)
            .Returns(ownerId);

        repository
            .Setup(currentRepository =>
                currentRepository.GetAllAsync(
                    ownerId,
                    It.IsAny<CancellationToken>()))
            .ReturnsAsync(
            [
                subscription
            ]);

        identityService
            .Setup(service =>
                service.GetBaseCurrencyAsync(
                    ownerId,
                    It.IsAny<CancellationToken>()))
            .ReturnsAsync(Currency.PLN);

        var handler =
            CreateHandler(
                repository,
                identityService,
                exchangeRateService,
                currentUser);

        var result =
            await handler.HandleAsync();

        Assert.Equal(
            Currency.PLN,
            result.BaseCurrency);

        Assert.Null(
            result.ExchangeRateEffectiveDate);

        Assert.Equal(
            1,
            result.ActiveCount);

        Assert.Equal(
            1,
            result.TotalCount);

        Assert.Equal(
            10m,
            result.MonthlyCost);

        Assert.Equal(
            120m,
            result.YearlyCost);

        Assert.Equal(
            10m,
            result.AverageMonthlyCost);

        Assert.Equal(
            120m,
            result.AverageYearlyCost);

        var topSubscription =
            Assert.Single(
                result.TopSubscriptions);

        Assert.Equal(
            subscription.Id,
            topSubscription.Id);

        Assert.Equal(
            10m,
            topSubscription.MonthlyCost);

        var activeSubscription =
            Assert.Single(
                result.ActiveSubscriptions);

        Assert.Equal(
            subscription.Id,
            activeSubscription.Id);

        Assert.Equal(
            10m,
            activeSubscription.MonthlyCost);

        var category =
            Assert.Single(
                result.Categories);

        Assert.Equal(
            DigitalServiceCategory.Video,
            category.Category);

        Assert.Null(
            category.CustomCategoryName);

        Assert.Equal(
            10m,
            category.MonthlyCost);

        exchangeRateService.Verify(
            service =>
                service.GetCurrentAsync(
                    It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task HandleAsync_ShouldConvertActiveSubscriptionsToBaseCurrency()
    {
        var ownerId =
            Guid.NewGuid();

        var netflix =
            CreateSubscription(
                ownerId,
                "Netflix",
                60m,
                Currency.PLN,
                BillingPeriod.Monthly,
                DigitalServiceCategory.Video);

        var spotify =
            CreateSubscription(
                ownerId,
                "Spotify",
                30m,
                Currency.EUR,
                BillingPeriod.Quarterly,
                DigitalServiceCategory.Music);

        var endedSubscription =
            CreateSubscription(
                ownerId,
                "Ended service",
                100m,
                Currency.USD,
                BillingPeriod.Monthly,
                DigitalServiceCategory.Other);

        endedSubscription.End(
            new DateOnly(2026, 7, 31));

        var repository =
            new Mock<ISubscriptionRepository>();

        var identityService =
            new Mock<IIdentityService>();

        var exchangeRateService =
            new Mock<IExchangeRateService>();

        var currentUser =
            new Mock<ICurrentUser>();

        currentUser
            .SetupGet(user =>
                user.UserId)
            .Returns(ownerId);

        repository
            .Setup(currentRepository =>
                currentRepository.GetAllAsync(
                    ownerId,
                    It.IsAny<CancellationToken>()))
            .ReturnsAsync(
            [
                netflix,
                spotify,
                endedSubscription
            ]);

        identityService
            .Setup(service =>
                service.GetBaseCurrencyAsync(
                    ownerId,
                    It.IsAny<CancellationToken>()))
            .ReturnsAsync(Currency.PLN);

        var effectiveDate =
            new DateOnly(2026, 8, 1);

        exchangeRateService
            .Setup(service =>
                service.GetCurrentAsync(
                    It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                new CurrentExchangeRates(
                    effectiveDate,
                    new Dictionary<Currency, decimal>
                    {
                        [Currency.PLN] = 1m,
                        [Currency.EUR] = 4m,
                        [Currency.USD] = 3.8m
                    }));

        var handler =
            CreateHandler(
                repository,
                identityService,
                exchangeRateService,
                currentUser);

        var result =
            await handler.HandleAsync();

        Assert.Equal(
            Currency.PLN,
            result.BaseCurrency);

        Assert.Equal(
            effectiveDate,
            result.ExchangeRateEffectiveDate);

        Assert.Equal(
            2,
            result.ActiveCount);

        Assert.Equal(
            3,
            result.TotalCount);

        Assert.Equal(
            100m,
            result.MonthlyCost);

        Assert.Equal(
            1200m,
            result.YearlyCost);

        Assert.Equal(
            50m,
            result.AverageMonthlyCost);

        Assert.Equal(
            600m,
            result.AverageYearlyCost);

        Assert.Collection(
            result.TopSubscriptions,
            first =>
            {
                Assert.Equal(
                    "Netflix",
                    first.Name);

                Assert.Equal(
                    60m,
                    first.MonthlyCost);
            },
            second =>
            {
                Assert.Equal(
                    "Spotify",
                    second.Name);

                Assert.Equal(
                    40m,
                    second.MonthlyCost);
            });

        Assert.Collection(
            result.ActiveSubscriptions,
            first =>
            {
                Assert.Equal(
                    "Netflix",
                    first.Name);

                Assert.Equal(
                    60m,
                    first.MonthlyCost);
            },
            second =>
            {
                Assert.Equal(
                    "Spotify",
                    second.Name);

                Assert.Equal(
                    40m,
                    second.MonthlyCost);
            });

        Assert.Collection(
            result.Categories,
            first =>
            {
                Assert.Equal(
                    DigitalServiceCategory.Video,
                    first.Category);

                Assert.Equal(
                    60m,
                    first.MonthlyCost);
            },
            second =>
            {
                Assert.Equal(
                    DigitalServiceCategory.Music,
                    second.Category);

                Assert.Equal(
                    40m,
                    second.MonthlyCost);
            });

        exchangeRateService.Verify(
            service =>
                service.GetCurrentAsync(
                    It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleAsync_ShouldReturnEmptySummary_WhenUserHasNoSubscriptions()
    {
        var ownerId =
            Guid.NewGuid();

        var repository =
            new Mock<ISubscriptionRepository>();

        var identityService =
            new Mock<IIdentityService>();

        var exchangeRateService =
            new Mock<IExchangeRateService>();

        var currentUser =
            new Mock<ICurrentUser>();

        currentUser
            .SetupGet(user =>
                user.UserId)
            .Returns(ownerId);

        repository
            .Setup(currentRepository =>
                currentRepository.GetAllAsync(
                    ownerId,
                    It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                Array.Empty<Subscription>());

        identityService
            .Setup(service =>
                service.GetBaseCurrencyAsync(
                    ownerId,
                    It.IsAny<CancellationToken>()))
            .ReturnsAsync(Currency.EUR);

        var handler =
            CreateHandler(
                repository,
                identityService,
                exchangeRateService,
                currentUser);

        var result =
            await handler.HandleAsync();

        Assert.Equal(
            Currency.EUR,
            result.BaseCurrency);

        Assert.Null(
            result.ExchangeRateEffectiveDate);

        Assert.Equal(
            0,
            result.ActiveCount);

        Assert.Equal(
            0,
            result.TotalCount);

        Assert.Equal(
            0m,
            result.MonthlyCost);

        Assert.Equal(
            0m,
            result.YearlyCost);

        Assert.Empty(
            result.TopSubscriptions);

        Assert.Empty(
            result.ActiveSubscriptions);

        Assert.Empty(
            result.Categories);

        exchangeRateService.Verify(
            service =>
                service.GetCurrentAsync(
                    It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task HandleAsync_ShouldThrow_WhenBaseCurrencyIsUnavailable()
    {
        var ownerId =
            Guid.NewGuid();

        var repository =
            new Mock<ISubscriptionRepository>();

        var identityService =
            new Mock<IIdentityService>();

        var exchangeRateService =
            new Mock<IExchangeRateService>();

        var currentUser =
            new Mock<ICurrentUser>();

        currentUser
            .SetupGet(user =>
                user.UserId)
            .Returns(ownerId);

        repository
            .Setup(currentRepository =>
                currentRepository.GetAllAsync(
                    ownerId,
                    It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                Array.Empty<Subscription>());

        identityService
            .Setup(service =>
                service.GetBaseCurrencyAsync(
                    ownerId,
                    It.IsAny<CancellationToken>()))
            .ReturnsAsync((Currency?)null);

        var handler =
            CreateHandler(
                repository,
                identityService,
                exchangeRateService,
                currentUser);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => handler.HandleAsync());

        exchangeRateService.Verify(
            service =>
                service.GetCurrentAsync(
                    It.IsAny<CancellationToken>()),
            Times.Never);
    }

    private static GetSubscriptionCostSummaryHandler CreateHandler(
        Mock<ISubscriptionRepository> repository,
        Mock<IIdentityService> identityService,
        Mock<IExchangeRateService> exchangeRateService,
        Mock<ICurrentUser> currentUser)
    {
        return new GetSubscriptionCostSummaryHandler(
            repository.Object,
            identityService.Object,
            exchangeRateService.Object,
            currentUser.Object);
    }

    private static Subscription CreateSubscription(
        Guid ownerId,
        string name,
        decimal amount,
        Currency currency,
        BillingPeriod billingPeriod,
        DigitalServiceCategory category)
    {
        var subscription =
            new Subscription(
                Guid.NewGuid(),
                ownerId,
                name,
                amount,
                currency,
                billingPeriod,
                new DateOnly(2026, 1, 1));

        subscription.AssignDigitalService(
            Guid.NewGuid(),
            category,
            null,
            null,
            null);

        return subscription;
    }
}
