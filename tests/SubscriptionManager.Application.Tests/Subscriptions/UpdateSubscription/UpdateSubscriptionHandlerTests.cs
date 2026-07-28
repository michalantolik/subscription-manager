using Moq;
using SubscriptionManager.Application.Common.Identity;
using SubscriptionManager.Application.DigitalServices;
using SubscriptionManager.Application.Subscriptions;
using SubscriptionManager.Application.Subscriptions.UpdateSubscription;
using SubscriptionManager.Domain.DigitalServices;
using SubscriptionManager.Domain.Subscriptions;

namespace SubscriptionManager.Application.Tests.Subscriptions.UpdateSubscription;

public sealed class UpdateSubscriptionHandlerTests
{
    [Fact]
    public async Task HandleAsync_ShouldUpdateCurrentUserSubscription_WhenSubscriptionExists()
    {
        var subscriptionId = Guid.NewGuid();
        var ownerId = Guid.NewGuid();

        var subscription = new Subscription(
            subscriptionId,
            ownerId,
            "Netflix",
            49m,
            "PLN",
            BillingPeriod.Monthly,
            new DateOnly(2026, 1, 1));

        var subscriptionRepository = new Mock<ISubscriptionRepository>();
        var digitalServiceRepository = new Mock<IDigitalServiceRepository>();
        var currentUser = new Mock<ICurrentUser>();

        currentUser
            .SetupGet(x => x.UserId)
            .Returns(ownerId);

        subscriptionRepository
            .Setup(x => x.GetByIdAsync(
                subscriptionId,
                ownerId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(subscription);

        var handler = new UpdateSubscriptionHandler(
            subscriptionRepository.Object,
            digitalServiceRepository.Object,
            currentUser.Object);

        var result = await handler.HandleAsync(
            new UpdateSubscriptionCommand(
                subscriptionId,
                "Spotify",
                59m,
                "eur",
                BillingPeriod.Yearly));

        Assert.True(result);
        Assert.Equal(ownerId, subscription.OwnerId);
        Assert.Null(subscription.DigitalServiceId);
        Assert.Equal("Spotify", subscription.Name);
        Assert.Equal(59m, subscription.Amount);
        Assert.Equal("EUR", subscription.Currency);
        Assert.Equal(BillingPeriod.Yearly, subscription.BillingPeriod);

        digitalServiceRepository.Verify(
            x => x.GetAvailableByIdAsync(
                It.IsAny<Guid>(),
                It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()),
            Times.Never);

        subscriptionRepository.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleAsync_ShouldAssignAvailableDigitalService()
    {
        var subscriptionId = Guid.NewGuid();
        var ownerId = Guid.NewGuid();
        var digitalServiceId = Guid.NewGuid();

        var subscription = new Subscription(
            subscriptionId,
            ownerId,
            "Manual name",
            49m,
            "PLN",
            BillingPeriod.Monthly,
            new DateOnly(2026, 1, 1));

        var digitalService = DigitalService.CreatePredefined(
            digitalServiceId,
            "netflix",
            "Netflix",
            DigitalServiceCategory.Video,
            "netflix",
            "https://www.netflix.com/account",
            10,
            DateTimeOffset.UtcNow);

        var subscriptionRepository = new Mock<ISubscriptionRepository>();
        var digitalServiceRepository = new Mock<IDigitalServiceRepository>();
        var currentUser = new Mock<ICurrentUser>();

        currentUser.SetupGet(x => x.UserId).Returns(ownerId);

        subscriptionRepository
            .Setup(x => x.GetByIdAsync(
                subscriptionId,
                ownerId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(subscription);

        digitalServiceRepository
            .Setup(x => x.GetAvailableByIdAsync(
                digitalServiceId,
                ownerId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(digitalService);

        var handler = new UpdateSubscriptionHandler(
            subscriptionRepository.Object,
            digitalServiceRepository.Object,
            currentUser.Object);

        var result = await handler.HandleAsync(
            new UpdateSubscriptionCommand(
                subscriptionId,
                "Personal Netflix",
                59m,
                "PLN",
                BillingPeriod.Monthly,
                digitalServiceId));

        Assert.True(result);
        Assert.Equal(digitalServiceId, subscription.DigitalServiceId);
        Assert.Equal("Personal Netflix", subscription.Name);
        Assert.Equal(DigitalServiceCategory.Video, subscription.Category);
        Assert.Equal("netflix", subscription.IconKey);
        Assert.Equal(
            "https://www.netflix.com/account",
            subscription.ManagementUrl);
    }

    [Fact]
    public async Task HandleAsync_ShouldReturnFalse_WhenCurrentUserSubscriptionDoesNotExist()
    {
        var subscriptionId = Guid.NewGuid();
        var ownerId = Guid.NewGuid();

        var subscriptionRepository = new Mock<ISubscriptionRepository>();
        var digitalServiceRepository = new Mock<IDigitalServiceRepository>();
        var currentUser = new Mock<ICurrentUser>();

        currentUser.SetupGet(x => x.UserId).Returns(ownerId);

        subscriptionRepository
            .Setup(x => x.GetByIdAsync(
                subscriptionId,
                ownerId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Subscription?)null);

        var handler = new UpdateSubscriptionHandler(
            subscriptionRepository.Object,
            digitalServiceRepository.Object,
            currentUser.Object);

        var result = await handler.HandleAsync(
            new UpdateSubscriptionCommand(
                subscriptionId,
                "Spotify",
                59m,
                "EUR",
                BillingPeriod.Yearly));

        Assert.False(result);

        subscriptionRepository.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task HandleAsync_ShouldThrow_WhenCommandIsNull()
    {
        var subscriptionRepository = new Mock<ISubscriptionRepository>();
        var digitalServiceRepository = new Mock<IDigitalServiceRepository>();
        var currentUser = new Mock<ICurrentUser>();

        var handler = new UpdateSubscriptionHandler(
            subscriptionRepository.Object,
            digitalServiceRepository.Object,
            currentUser.Object);

        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            handler.HandleAsync(null!));

        subscriptionRepository.Verify(
            x => x.GetByIdAsync(
                It.IsAny<Guid>(),
                It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }
}
