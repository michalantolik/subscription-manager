using Moq;
using SubscriptionManager.Application.Common.Identity;
using SubscriptionManager.Application.Subscriptions;
using SubscriptionManager.Application.Subscriptions.DeleteSubscription;
using SubscriptionManager.Domain.Subscriptions;

namespace SubscriptionManager.Application.Tests.Subscriptions.DeleteSubscription;

public sealed class DeleteSubscriptionHandlerTests
{
    [Fact]
    public async Task HandleAsync_ShouldDeleteCurrentUserSubscription_WhenSubscriptionExists()
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

        var handler = new DeleteSubscriptionHandler(
            repository.Object,
            currentUser.Object);

        var result = await handler.HandleAsync(
            new DeleteSubscriptionCommand(subscriptionId));

        Assert.True(result);

        repository.Verify(
            x => x.GetByIdAsync(
                subscriptionId,
                ownerId,
                It.IsAny<CancellationToken>()),
            Times.Once);

        repository.Verify(
            x => x.Remove(subscription),
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

        var handler = new DeleteSubscriptionHandler(
            repository.Object,
            currentUser.Object);

        var result = await handler.HandleAsync(
            new DeleteSubscriptionCommand(subscriptionId));

        Assert.False(result);

        repository.Verify(
            x => x.GetByIdAsync(
                subscriptionId,
                ownerId,
                It.IsAny<CancellationToken>()),
            Times.Once);

        repository.Verify(
            x => x.Remove(It.IsAny<Subscription>()),
            Times.Never);

        repository.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task HandleAsync_ShouldThrow_WhenCommandIsNull()
    {
        var repository = new Mock<ISubscriptionRepository>();
        var currentUser = new Mock<ICurrentUser>();

        var handler = new DeleteSubscriptionHandler(
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
            x => x.Remove(It.IsAny<Subscription>()),
            Times.Never);

        repository.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }
}
