using Moq;
using SubscriptionManager.Application.Common.Identity;
using SubscriptionManager.Application.ExchangeRates;
using SubscriptionManager.Application.SavingsPlans;
using SubscriptionManager.Application.SavingsPlans.CreateSavingsPlan;
using SubscriptionManager.Application.Subscriptions;
using SubscriptionManager.Domain.Billing;
using SubscriptionManager.Domain.DigitalServices;
using SubscriptionManager.Domain.Subscriptions;

namespace SubscriptionManager.Application.Tests.SavingsPlans.CreateSavingsPlan;

public sealed class CreateSavingsPlanHandlerTests
{
    [Fact]
    public async Task HandleAsync_ShouldCalculateScenarioUsingApplicationData()
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

        var repository =
            new Mock<ISubscriptionRepository>();

        var identityService =
            new Mock<IIdentityService>();

        var exchangeRateService =
            new Mock<IExchangeRateService>();

        var currentUser =
            new Mock<ICurrentUser>();

        var savingsPlanAgent =
            new Mock<ISavingsPlanAgent>();

        var savingsPlanUsageRepository =
            new Mock<ISavingsPlanUsageRepository>();

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
                spotify
            ]);

        identityService
            .Setup(service =>
                service.GetBaseCurrencyAsync(
                    ownerId,
                    It.IsAny<CancellationToken>()))
            .ReturnsAsync(Currency.PLN);

        identityService
            .Setup(service =>
                service.GetSubscriptionPlanAsync(
                    ownerId,
                    It.IsAny<CancellationToken>()))
            .ReturnsAsync(SubscriptionPlan.Plus);

        exchangeRateService
            .Setup(service =>
                service.GetCurrentAsync(
                    It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                new CurrentExchangeRates(
                    new DateOnly(2026, 8, 3),
                    new Dictionary<Currency, decimal>
                    {
                        [Currency.PLN] = 1m,
                        [Currency.EUR] = 4m
                    }));

        savingsPlanUsageRepository
            .Setup(currentRepository =>
                currentRepository.TryRegisterRequestAsync(
                    ownerId,
                    It.IsAny<DateOnly>(),
                    SubscriptionPlanLimits
                        .PlusDailySavingsPlanLimit,
                    It.IsAny<CancellationToken>()))
            .ReturnsAsync(3);

        SavingsPlanAgentRequest? capturedRequest = null;

        savingsPlanAgent
            .Setup(agent =>
                agent.CreatePlanAsync(
                    It.IsAny<SavingsPlanAgentRequest>(),
                    It.IsAny<CancellationToken>()))
            .Callback<SavingsPlanAgentRequest, CancellationToken>(
                (request, _) =>
                    capturedRequest = request)
            .ReturnsAsync(
                new SavingsPlanAgentResult(
                    new SavingsPlanAgentScenario(
                        [netflix.Id],
                        "Ending Netflix reaches the selected budget."),
                    null));

        var handler =
            CreateHandler(
                repository,
                identityService,
                exchangeRateService,
                currentUser,
                savingsPlanAgent,
                savingsPlanUsageRepository);

        var command =
            new CreateSavingsPlanCommand(
                SavingsPlanGoalType.MonthlyBudget,
                50m,
                [spotify.Id],
                SavingsPlanStrategy.Balanced,
                "Keep music services.",
                "en");

        var result =
            await handler.HandleAsync(command);

        Assert.Equal(
            Currency.PLN,
            result.BaseCurrency);

        Assert.Equal(
            100m,
            result.CurrentMonthlyCost);

        Assert.Equal(
            SubscriptionPlan.Plus,
            result.SubscriptionPlan);

        Assert.Equal(
            SubscriptionPlanLimits
                .PlusDailySavingsPlanLimit,
            result.DailyRequestLimit);

        Assert.Equal(
            3,
            result.RemainingRequestCount);

        var recommended =
            Assert.IsType<SavingsPlanScenarioDto>(
                result.Recommended);

        Assert.Equal(
            40m,
            recommended.ProjectedMonthlyCost);

        Assert.Equal(
            60m,
            recommended.MonthlySavings);

        Assert.Equal(
            720m,
            recommended.YearlySavings);

        Assert.True(
            recommended.TargetReached);

        var selectedSubscription =
            Assert.Single(
                recommended.Subscriptions);

        Assert.Equal(
            netflix.Id,
            selectedSubscription.Id);

        Assert.Equal(
            60m,
            selectedSubscription.MonthlyCost);

        Assert.NotNull(
            capturedRequest);

        Assert.Equal(
            100m,
            capturedRequest.CurrentMonthlyCost);

        Assert.Equal(
            "en",
            capturedRequest.LanguageCode);

        Assert.Contains(
            capturedRequest.Subscriptions,
            subscription =>
                subscription.Id == spotify.Id &&
                subscription.MonthlyCost == 40m);

        Assert.Contains(
            spotify.Id,
            capturedRequest.ProtectedSubscriptionIds);

        exchangeRateService.Verify(
            service =>
                service.GetCurrentAsync(
                    It.IsAny<CancellationToken>()),
            Times.Once);

        savingsPlanUsageRepository.Verify(
            currentRepository =>
                currentRepository.TryRegisterRequestAsync(
                    ownerId,
                    It.IsAny<DateOnly>(),
                    SubscriptionPlanLimits
                        .PlusDailySavingsPlanLimit,
                    It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleAsync_ShouldRejectProtectedSubscriptionReturnedByAgent()
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
                40m,
                Currency.PLN,
                BillingPeriod.Monthly,
                DigitalServiceCategory.Music);

        var repository =
            new Mock<ISubscriptionRepository>();

        var identityService =
            new Mock<IIdentityService>();

        var exchangeRateService =
            new Mock<IExchangeRateService>();

        var currentUser =
            new Mock<ICurrentUser>();

        var savingsPlanAgent =
            new Mock<ISavingsPlanAgent>();

        var savingsPlanUsageRepository =
            new Mock<ISavingsPlanUsageRepository>();

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
                spotify
            ]);

        identityService
            .Setup(service =>
                service.GetBaseCurrencyAsync(
                    ownerId,
                    It.IsAny<CancellationToken>()))
            .ReturnsAsync(Currency.PLN);

        identityService
            .Setup(service =>
                service.GetSubscriptionPlanAsync(
                    ownerId,
                    It.IsAny<CancellationToken>()))
            .ReturnsAsync(SubscriptionPlan.Plus);

        savingsPlanUsageRepository
            .Setup(currentRepository =>
                currentRepository.TryRegisterRequestAsync(
                    ownerId,
                    It.IsAny<DateOnly>(),
                    SubscriptionPlanLimits
                        .PlusDailySavingsPlanLimit,
                    It.IsAny<CancellationToken>()))
            .ReturnsAsync(3);

        savingsPlanAgent
            .Setup(agent =>
                agent.CreatePlanAsync(
                    It.IsAny<SavingsPlanAgentRequest>(),
                    It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                new SavingsPlanAgentResult(
                    new SavingsPlanAgentScenario(
                        [spotify.Id],
                        "End Spotify."),
                    null));

        var handler =
            CreateHandler(
                repository,
                identityService,
                exchangeRateService,
                currentUser,
                savingsPlanAgent,
                savingsPlanUsageRepository);

        var command =
            new CreateSavingsPlanCommand(
                SavingsPlanGoalType.MonthlyBudget,
                50m,
                [spotify.Id],
                SavingsPlanStrategy.Balanced,
                null,
                "en");

        var exception =
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => handler.HandleAsync(command));

        Assert.Equal(
            "The savings plan agent returned an invalid result.",
            exception.Message);

        exchangeRateService.Verify(
            service =>
                service.GetCurrentAsync(
                    It.IsAny<CancellationToken>()),
            Times.Never);

        savingsPlanUsageRepository.Verify(
            currentRepository =>
                currentRepository.TryRegisterRequestAsync(
                    ownerId,
                    It.IsAny<DateOnly>(),
                    SubscriptionPlanLimits
                        .PlusDailySavingsPlanLimit,
                    It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleAsync_ShouldRejectRequest_WhenDailyLimitIsReached()
    {
        var ownerId =
            Guid.NewGuid();

        var subscription =
            CreateSubscription(
                ownerId,
                "Netflix",
                60m,
                Currency.PLN,
                BillingPeriod.Monthly,
                DigitalServiceCategory.Video);

        var repository =
            new Mock<ISubscriptionRepository>();

        var identityService =
            new Mock<IIdentityService>();

        var exchangeRateService =
            new Mock<IExchangeRateService>();

        var currentUser =
            new Mock<ICurrentUser>();

        var savingsPlanAgent =
            new Mock<ISavingsPlanAgent>();

        var savingsPlanUsageRepository =
            new Mock<ISavingsPlanUsageRepository>();

        currentUser
            .SetupGet(user =>
                user.UserId)
            .Returns(ownerId);

        repository
            .Setup(currentRepository =>
                currentRepository.GetAllAsync(
                    ownerId,
                    It.IsAny<CancellationToken>()))
            .ReturnsAsync([subscription]);

        identityService
            .Setup(service =>
                service.GetBaseCurrencyAsync(
                    ownerId,
                    It.IsAny<CancellationToken>()))
            .ReturnsAsync(Currency.PLN);

        identityService
            .Setup(service =>
                service.GetSubscriptionPlanAsync(
                    ownerId,
                    It.IsAny<CancellationToken>()))
            .ReturnsAsync(SubscriptionPlan.Plus);

        savingsPlanUsageRepository
            .Setup(currentRepository =>
                currentRepository.TryRegisterRequestAsync(
                    ownerId,
                    It.IsAny<DateOnly>(),
                    SubscriptionPlanLimits
                        .PlusDailySavingsPlanLimit,
                    It.IsAny<CancellationToken>()))
            .ReturnsAsync((int?)null);

        var handler =
            CreateHandler(
                repository,
                identityService,
                exchangeRateService,
                currentUser,
                savingsPlanAgent,
                savingsPlanUsageRepository);

        var command =
            new CreateSavingsPlanCommand(
                SavingsPlanGoalType.MonthlySavings,
                20m,
                [],
                SavingsPlanStrategy.Balanced,
                null,
                "en");

        var exception =
            await Assert.ThrowsAsync<
                SavingsPlanUsageLimitExceededException>(
                () => handler.HandleAsync(command));

        Assert.Equal(
            SubscriptionPlanLimits
                .PlusDailySavingsPlanLimit,
            exception.DailyLimit);

        savingsPlanAgent.Verify(
            agent =>
                agent.CreatePlanAsync(
                    It.IsAny<SavingsPlanAgentRequest>(),
                    It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task HandleAsync_ShouldCreateSavingsPlanForFreeUser()
    {
        var ownerId =
            Guid.NewGuid();

        var subscription =
            CreateSubscription(
                ownerId,
                "Netflix",
                60m,
                Currency.PLN,
                BillingPeriod.Monthly,
                DigitalServiceCategory.Video);

        var repository =
            new Mock<ISubscriptionRepository>();

        var identityService =
            new Mock<IIdentityService>();

        var exchangeRateService =
            new Mock<IExchangeRateService>();

        var currentUser =
            new Mock<ICurrentUser>();

        var savingsPlanAgent =
            new Mock<ISavingsPlanAgent>();

        var savingsPlanUsageRepository =
            new Mock<ISavingsPlanUsageRepository>();

        currentUser
            .SetupGet(user =>
                user.UserId)
            .Returns(ownerId);

        repository
            .Setup(currentRepository =>
                currentRepository.GetAllAsync(
                    ownerId,
                    It.IsAny<CancellationToken>()))
            .ReturnsAsync([subscription]);

        identityService
            .Setup(service =>
                service.GetBaseCurrencyAsync(
                    ownerId,
                    It.IsAny<CancellationToken>()))
            .ReturnsAsync(Currency.PLN);

        identityService
            .Setup(service =>
                service.GetSubscriptionPlanAsync(
                    ownerId,
                    It.IsAny<CancellationToken>()))
            .ReturnsAsync(SubscriptionPlan.Free);

        savingsPlanUsageRepository
            .Setup(currentRepository =>
                currentRepository.TryRegisterRequestAsync(
                    ownerId,
                    It.IsAny<DateOnly>(),
                    SubscriptionPlanLimits
                        .FreeDailySavingsPlanLimit,
                    It.IsAny<CancellationToken>()))
            .ReturnsAsync(2);

        savingsPlanAgent
            .Setup(agent =>
                agent.CreatePlanAsync(
                    It.IsAny<SavingsPlanAgentRequest>(),
                    It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                new SavingsPlanAgentResult(
                    new SavingsPlanAgentScenario(
                        [subscription.Id],
                        "Ending Netflix reaches the selected savings target."),
                    null));

        var handler =
            CreateHandler(
                repository,
                identityService,
                exchangeRateService,
                currentUser,
                savingsPlanAgent,
                savingsPlanUsageRepository);

        var command =
            new CreateSavingsPlanCommand(
                SavingsPlanGoalType.MonthlySavings,
                20m,
                [],
                SavingsPlanStrategy.Balanced,
                null,
                "en");

        var result =
            await handler.HandleAsync(command);

        Assert.Equal(
            SubscriptionPlan.Free,
            result.SubscriptionPlan);

        Assert.Equal(
            SubscriptionPlanLimits
                .FreeDailySavingsPlanLimit,
            result.DailyRequestLimit);

        Assert.Equal(
            2,
            result.RemainingRequestCount);

        var recommended =
            Assert.IsType<SavingsPlanScenarioDto>(
                result.Recommended);

        Assert.Equal(
            0m,
            recommended.ProjectedMonthlyCost);

        Assert.Equal(
            60m,
            recommended.MonthlySavings);

        Assert.Equal(
            720m,
            recommended.YearlySavings);

        Assert.True(
            recommended.TargetReached);

        var selectedSubscription =
            Assert.Single(
                recommended.Subscriptions);

        Assert.Equal(
            subscription.Id,
            selectedSubscription.Id);

        savingsPlanUsageRepository.Verify(
            currentRepository =>
                currentRepository.TryRegisterRequestAsync(
                    ownerId,
                    It.IsAny<DateOnly>(),
                    SubscriptionPlanLimits
                        .FreeDailySavingsPlanLimit,
                    It.IsAny<CancellationToken>()),
            Times.Once);

        savingsPlanAgent.Verify(
            agent =>
                agent.CreatePlanAsync(
                    It.IsAny<SavingsPlanAgentRequest>(),
                    It.IsAny<CancellationToken>()),
            Times.Once);
    }

    private static CreateSavingsPlanHandler CreateHandler(
        Mock<ISubscriptionRepository> repository,
        Mock<IIdentityService> identityService,
        Mock<IExchangeRateService> exchangeRateService,
        Mock<ICurrentUser> currentUser,
        Mock<ISavingsPlanAgent> savingsPlanAgent,
        Mock<ISavingsPlanUsageRepository>
            savingsPlanUsageRepository)
    {
        return new CreateSavingsPlanHandler(
            repository.Object,
            identityService.Object,
            exchangeRateService.Object,
            currentUser.Object,
            savingsPlanAgent.Object,
            savingsPlanUsageRepository.Object);
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
