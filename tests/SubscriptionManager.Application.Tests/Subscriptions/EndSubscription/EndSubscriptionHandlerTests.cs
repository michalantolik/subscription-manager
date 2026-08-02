using Moq;
using SubscriptionManager.Application.Common.Identity;
using SubscriptionManager.Application.Subscriptions;
using SubscriptionManager.Application.Subscriptions.EndSubscription;
using SubscriptionManager.Domain.Subscriptions;

namespace SubscriptionManager.Application.Tests.Subscriptions.EndSubscription;

public sealed class EndSubscriptionHandlerTests
{
    [Fact]
    public async Task HandleAsync_ShouldEndCurrentUserSubscription_WhenSubscriptionExists()
    {
        var subscriptionId = Guid.NewGuid();
        var ownerId = Guid.NewGuid();
        var endDate = new DateOnly(2026, 7, 21);

        var subscription = new Subscription(
            subscriptionId,
            ownerId,
            "Netflix",
            49m,
            Currency.PLN,
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

        var handler = new EndSubscriptionHandler(
            repository.Object,
            currentUser.Object);

        var result = await handler.HandleAsync(
            new EndSubscriptionCommand(
                subscriptionId,
                endDate));

        Assert.True(result);
        Assert.False(subscription.IsActive);
        Assert.Equal(endDate, subscription.EndDate);

        repository.Verify(
            x => x.GetByIdAsync(
                subscriptionId,
                ownerId,
                It.IsAny<CancellationToken>()),
            Times.Once);

        repository.Verify(
            x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
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

        var handler = new EndSubscriptionHandler(
            repository.Object,
            currentUser.Object);

        var result = await handler.HandleAsync(
            new EndSubscriptionCommand(
                subscriptionId,
                new DateOnly(2026, 7, 21)));

        Assert.False(result);

        repository.Verify(
            x => x.GetByIdAsync(
                subscriptionId,
                ownerId,
                It.IsAny<CancellationToken>()),
            Times.Once);

        repository.Verify(
            x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task HandleAsync_ShouldThrow_WhenCommandIsNull()
    {
        var repository = new Mock<ISubscriptionRepository>();
        var currentUser = new Mock<ICurrentUser>();

        var handler = new EndSubscriptionHandler(
            repository.Object,
            currentUser.Object);

        await Assert.ThrowsAsync<ArgumentNullException>(
            () => handler.HandleAsync(null!));

        repository.Verify(
            x => x.GetByIdAsync(
                It.IsAny<Guid>(),
                It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()),
            Times.Never);

        repository.Verify(
            x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Never);
    }
}
