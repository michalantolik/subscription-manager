using Moq;
using SubscriptionManager.Application.Common.Identity;
using SubscriptionManager.Application.DigitalServices;
using SubscriptionManager.Application.Subscriptions;
using SubscriptionManager.Application.Subscriptions.CreateSubscription;
using SubscriptionManager.Domain.Billing;
using SubscriptionManager.Domain.DigitalServices;
using SubscriptionManager.Domain.Subscriptions;

namespace SubscriptionManager.Application.Tests.Subscriptions.CreateSubscription;

public sealed class CreateSubscriptionHandlerTests
{
    [Fact]
    public async Task HandleAsync_ShouldCreateManualSubscriptionForCurrentUser()
    {
        var ownerId = Guid.NewGuid();

        var subscriptionRepository =
            new Mock<ISubscriptionRepository>();

        var digitalServiceRepository =
            new Mock<IDigitalServiceRepository>();

        var identityService =
            new Mock<IIdentityService>();

        var currentUser =
            new Mock<ICurrentUser>();

        currentUser
            .SetupGet(x => x.UserId)
            .Returns(ownerId);

        identityService
            .Setup(x => x.GetSubscriptionPlanAsync(
                ownerId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(SubscriptionPlan.Free);

        subscriptionRepository
            .Setup(x => x.GetActiveCountAsync(
                ownerId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(0);

        Subscription? addedSubscription = null;

        subscriptionRepository
            .Setup(x => x.AddAsync(
                It.IsAny<Subscription>(),
                It.IsAny<CancellationToken>()))
            .Callback<Subscription, CancellationToken>(
                (subscription, _) =>
                    addedSubscription = subscription)
            .Returns(Task.CompletedTask);

        var handler = new CreateSubscriptionHandler(
            subscriptionRepository.Object,
            digitalServiceRepository.Object,
            identityService.Object,
            currentUser.Object);

        var result = await handler.HandleAsync(
            new CreateSubscriptionCommand(
                "  Netflix  ",
                49m,
                Currency.PLN,
                BillingPeriod.Monthly,
                new DateOnly(2026, 1, 1)));

        Assert.NotEqual(Guid.Empty, result);
        Assert.NotNull(addedSubscription);
        Assert.Equal(result, addedSubscription.Id);
        Assert.Equal(ownerId, addedSubscription.OwnerId);
        Assert.Null(addedSubscription.DigitalServiceId);
        Assert.Equal("Netflix", addedSubscription.Name);
        Assert.Null(addedSubscription.Category);
        Assert.Null(addedSubscription.CustomCategoryName);
        Assert.Null(addedSubscription.IconKey);
        Assert.Null(addedSubscription.ManagementUrl);
        Assert.Equal(49m, addedSubscription.Amount);
        Assert.Equal(
            Currency.PLN,
            addedSubscription.Currency);

        Assert.Equal(
            BillingPeriod.Monthly,
            addedSubscription.BillingPeriod);

        Assert.Equal(
            new DateOnly(2026, 1, 1),
            addedSubscription.StartDate);

        digitalServiceRepository.Verify(
            x => x.GetAvailableByIdAsync(
                It.IsAny<Guid>(),
                It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()),
            Times.Never);

        subscriptionRepository.Verify(
            x => x.AddAsync(
                addedSubscription,
                It.IsAny<CancellationToken>()),
            Times.Once);

        subscriptionRepository.Verify(
            x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleAsync_ShouldCreateSubscriptionFromAvailableDigitalService()
    {
        var ownerId = Guid.NewGuid();
        var digitalServiceId = Guid.NewGuid();
        var createdAt = DateTimeOffset.UtcNow;

        var digitalService =
            DigitalService.CreatePredefined(
                digitalServiceId,
                "netflix",
                "Netflix",
                DigitalServiceCategory.Video,
                "netflix",
                "https://www.netflix.com/account",
                10,
                createdAt);

        var subscriptionRepository =
            new Mock<ISubscriptionRepository>();

        var digitalServiceRepository =
            new Mock<IDigitalServiceRepository>();

        var identityService =
            new Mock<IIdentityService>();

        var currentUser =
            new Mock<ICurrentUser>();

        currentUser
            .SetupGet(x => x.UserId)
            .Returns(ownerId);

        identityService
            .Setup(x => x.GetSubscriptionPlanAsync(
                ownerId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(SubscriptionPlan.Free);

        subscriptionRepository
            .Setup(x => x.GetActiveCountAsync(
                ownerId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(0);

        digitalServiceRepository
            .Setup(x => x.GetAvailableByIdAsync(
                digitalServiceId,
                ownerId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(digitalService);

        Subscription? addedSubscription = null;

        subscriptionRepository
            .Setup(x => x.AddAsync(
                It.IsAny<Subscription>(),
                It.IsAny<CancellationToken>()))
            .Callback<Subscription, CancellationToken>(
                (subscription, _) =>
                    addedSubscription = subscription)
            .Returns(Task.CompletedTask);

        var handler = new CreateSubscriptionHandler(
            subscriptionRepository.Object,
            digitalServiceRepository.Object,
            identityService.Object,
            currentUser.Object);

        var result = await handler.HandleAsync(
            new CreateSubscriptionCommand(
                "Personal Netflix",
                49m,
                Currency.PLN,
                BillingPeriod.Monthly,
                new DateOnly(2026, 1, 1),
                digitalServiceId));

        Assert.NotEqual(Guid.Empty, result);
        Assert.NotNull(addedSubscription);
        Assert.Equal(result, addedSubscription.Id);
        Assert.Equal(ownerId, addedSubscription.OwnerId);

        Assert.Equal(
            digitalServiceId,
            addedSubscription.DigitalServiceId);

        Assert.Equal(
            "Personal Netflix",
            addedSubscription.Name);

        Assert.Equal(
            DigitalServiceCategory.Video,
            addedSubscription.Category);

        Assert.Null(
            addedSubscription.CustomCategoryName);

        Assert.Equal(
            "netflix",
            addedSubscription.IconKey);

        Assert.Equal(
            "https://www.netflix.com/account",
            addedSubscription.ManagementUrl);

        digitalServiceRepository.Verify(
            x => x.GetAvailableByIdAsync(
                digitalServiceId,
                ownerId,
                It.IsAny<CancellationToken>()),
            Times.Once);

        subscriptionRepository.Verify(
            x => x.AddAsync(
                addedSubscription,
                It.IsAny<CancellationToken>()),
            Times.Once);

        subscriptionRepository.Verify(
            x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleAsync_ShouldThrow_WhenDigitalServiceIsNotAvailable()
    {
        var ownerId = Guid.NewGuid();
        var digitalServiceId = Guid.NewGuid();

        var subscriptionRepository =
            new Mock<ISubscriptionRepository>();

        var digitalServiceRepository =
            new Mock<IDigitalServiceRepository>();

        var identityService =
            new Mock<IIdentityService>();

        var currentUser =
            new Mock<ICurrentUser>();

        currentUser
            .SetupGet(x => x.UserId)
            .Returns(ownerId);

        identityService
            .Setup(x => x.GetSubscriptionPlanAsync(
                ownerId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(SubscriptionPlan.Free);

        subscriptionRepository
            .Setup(x => x.GetActiveCountAsync(
                ownerId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(0);

        digitalServiceRepository
            .Setup(x => x.GetAvailableByIdAsync(
                digitalServiceId,
                ownerId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((DigitalService?)null);

        var handler = new CreateSubscriptionHandler(
            subscriptionRepository.Object,
            digitalServiceRepository.Object,
            identityService.Object,
            currentUser.Object);

        var exception =
            await Assert.ThrowsAsync<ArgumentException>(
                () => handler.HandleAsync(
                    new CreateSubscriptionCommand(
                        "Netflix",
                        49m,
                        Currency.PLN,
                        BillingPeriod.Monthly,
                        new DateOnly(2026, 1, 1),
                        digitalServiceId)));

        Assert.Equal(
            "DigitalServiceId",
            exception.ParamName);

        subscriptionRepository.Verify(
            x => x.AddAsync(
                It.IsAny<Subscription>(),
                It.IsAny<CancellationToken>()),
            Times.Never);

        subscriptionRepository.Verify(
            x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task HandleAsync_ShouldAllowFifthActiveSubscriptionForFreePlan()
    {
        var ownerId = Guid.NewGuid();

        var subscriptionRepository =
            new Mock<ISubscriptionRepository>();

        var digitalServiceRepository =
            new Mock<IDigitalServiceRepository>();

        var identityService =
            new Mock<IIdentityService>();

        var currentUser =
            new Mock<ICurrentUser>();

        currentUser
            .SetupGet(x => x.UserId)
            .Returns(ownerId);

        identityService
            .Setup(x => x.GetSubscriptionPlanAsync(
                ownerId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(SubscriptionPlan.Free);

        subscriptionRepository
            .Setup(x => x.GetActiveCountAsync(
                ownerId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                SubscriptionPlanLimits.FreeSubscriptionLimit - 1);

        var handler = new CreateSubscriptionHandler(
            subscriptionRepository.Object,
            digitalServiceRepository.Object,
            identityService.Object,
            currentUser.Object);

        var result = await handler.HandleAsync(
            new CreateSubscriptionCommand(
                "Netflix",
                49m,
                Currency.PLN,
                BillingPeriod.Monthly,
                new DateOnly(2026, 1, 1)));

        Assert.NotEqual(Guid.Empty, result);

        subscriptionRepository.Verify(
            x => x.AddAsync(
                It.IsAny<Subscription>(),
                It.IsAny<CancellationToken>()),
            Times.Once);

        subscriptionRepository.Verify(
            x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleAsync_ShouldThrow_WhenFreePlanSubscriptionLimitIsReached()
    {
        var ownerId = Guid.NewGuid();

        var subscriptionRepository =
            new Mock<ISubscriptionRepository>();

        var digitalServiceRepository =
            new Mock<IDigitalServiceRepository>();

        var identityService =
            new Mock<IIdentityService>();

        var currentUser =
            new Mock<ICurrentUser>();

        currentUser
            .SetupGet(x => x.UserId)
            .Returns(ownerId);

        identityService
            .Setup(x => x.GetSubscriptionPlanAsync(
                ownerId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(SubscriptionPlan.Free);

        subscriptionRepository
            .Setup(x => x.GetActiveCountAsync(
                ownerId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                SubscriptionPlanLimits.FreeSubscriptionLimit);

        var handler = new CreateSubscriptionHandler(
            subscriptionRepository.Object,
            digitalServiceRepository.Object,
            identityService.Object,
            currentUser.Object);

        var exception =
            await Assert.ThrowsAsync<SubscriptionLimitReachedException>(
                () => handler.HandleAsync(
                    new CreateSubscriptionCommand(
                        "Netflix",
                        49m,
                        Currency.PLN,
                        BillingPeriod.Monthly,
                        new DateOnly(2026, 1, 1))));

        Assert.Equal(
            SubscriptionPlanLimits.FreeSubscriptionLimit,
            exception.Limit);

        digitalServiceRepository.Verify(
            x => x.GetAvailableByIdAsync(
                It.IsAny<Guid>(),
                It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()),
            Times.Never);

        subscriptionRepository.Verify(
            x => x.AddAsync(
                It.IsAny<Subscription>(),
                It.IsAny<CancellationToken>()),
            Times.Never);

        subscriptionRepository.Verify(
            x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task HandleAsync_ShouldThrow_WhenCommandIsNull()
    {
        var subscriptionRepository =
            new Mock<ISubscriptionRepository>();

        var digitalServiceRepository =
            new Mock<IDigitalServiceRepository>();

        var identityService =
            new Mock<IIdentityService>();

        var currentUser =
            new Mock<ICurrentUser>();

        var handler = new CreateSubscriptionHandler(
            subscriptionRepository.Object,
            digitalServiceRepository.Object,
            identityService.Object,
            currentUser.Object);

        await Assert.ThrowsAsync<ArgumentNullException>(
            () => handler.HandleAsync(null!));

        digitalServiceRepository.Verify(
            x => x.GetAvailableByIdAsync(
                It.IsAny<Guid>(),
                It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()),
            Times.Never);

        subscriptionRepository.Verify(
            x => x.AddAsync(
                It.IsAny<Subscription>(),
                It.IsAny<CancellationToken>()),
            Times.Never);

        subscriptionRepository.Verify(
            x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Never);
    }
}
