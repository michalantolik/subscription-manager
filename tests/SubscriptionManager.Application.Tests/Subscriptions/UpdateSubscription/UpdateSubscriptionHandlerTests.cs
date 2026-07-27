using Moq;
using SubscriptionManager.Application.Common.Authentication;
using SubscriptionManager.Application.Subscriptions;
using SubscriptionManager.Application.Subscriptions.UpdateSubscription;
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

        var repository = new Mock<ISubscriptionRepository>();
        var currentUser = new Mock<ICurrentUser>();

        currentUser
            .SetupGet(x => x.UserId)
            .Returns(ownerId);

        repository
            .Setup(x => x.GetByIdAsync(
                subscriptionId,
                ownerId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(subscription);

        var handler = new UpdateSubscriptionHandler(
            repository.Object,
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
        Assert.Equal("Spotify", subscription.Name);
        Assert.Equal(59m, subscription.Amount);
        Assert.Equal("EUR", subscription.Currency);
        Assert.Equal(BillingPeriod.Yearly, subscription.BillingPeriod);

        repository.Verify(
            x => x.GetByIdAsync(
                subscriptionId,
                ownerId,
                It.IsAny<CancellationToken>()),
            Times.Once);

        repository.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleAsync_ShouldReturnFalse_WhenCurrentUserSubscriptionDoesNotExist()
    {
        var subscriptionId = Guid.NewGuid();
        var ownerId = Guid.NewGuid();

        var repository = new Mock<ISubscriptionRepository>();
        var currentUser = new Mock<ICurrentUser>();

        currentUser
            .SetupGet(x => x.UserId)
            .Returns(ownerId);

        repository
            .Setup(x => x.GetByIdAsync(
                subscriptionId,
                ownerId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Subscription?)null);

        var handler = new UpdateSubscriptionHandler(
            repository.Object,
            currentUser.Object);

        var result = await handler.HandleAsync(
            new UpdateSubscriptionCommand(
                subscriptionId,
                "Spotify",
                59m,
                "EUR",
                BillingPeriod.Yearly));

        Assert.False(result);

        repository.Verify(
            x => x.GetByIdAsync(
                subscriptionId,
                ownerId,
                It.IsAny<CancellationToken>()),
            Times.Once);

        repository.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task HandleAsync_ShouldThrow_WhenCommandIsNull()
    {
        var repository = new Mock<ISubscriptionRepository>();
        var currentUser = new Mock<ICurrentUser>();

        var handler = new UpdateSubscriptionHandler(
            repository.Object,
            currentUser.Object);

        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            handler.HandleAsync(null!));

        repository.Verify(
            x => x.GetByIdAsync(
                It.IsAny<Guid>(),
                It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()),
            Times.Never);

        repository.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }
}
