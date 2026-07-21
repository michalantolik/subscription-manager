using Moq;
using SubscriptionManager.Application.Subscriptions;
using SubscriptionManager.Application.Subscriptions.DeleteSubscription;
using SubscriptionManager.Domain.Subscriptions;

namespace SubscriptionManager.Application.Tests.Subscriptions.DeleteSubscription;

public sealed class DeleteSubscriptionHandlerTests
{
    [Fact]
    public async Task HandleAsync_ShouldDeleteSubscription_WhenSubscriptionExists()
    {
        var subscriptionId = Guid.NewGuid();

        var subscription = new Subscription(
            subscriptionId,
            "Netflix",
            49m,
            "PLN",
            BillingPeriod.Monthly,
            new DateOnly(2026, 1, 1));

        var repository = new Mock<ISubscriptionRepository>();

        repository
            .Setup(x => x.GetByIdAsync(
                subscriptionId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(subscription);

        var handler = new DeleteSubscriptionHandler(repository.Object);

        var result = await handler.HandleAsync(
            new DeleteSubscriptionCommand(subscriptionId));

        Assert.True(result);

        repository.Verify(
            x => x.Remove(subscription),
            Times.Once);

        repository.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleAsync_ShouldReturnFalse_WhenSubscriptionDoesNotExist()
    {
        var subscriptionId = Guid.NewGuid();

        var repository = new Mock<ISubscriptionRepository>();

        repository
            .Setup(x => x.GetByIdAsync(
                subscriptionId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Subscription?)null);

        var handler = new DeleteSubscriptionHandler(repository.Object);

        var result = await handler.HandleAsync(
            new DeleteSubscriptionCommand(subscriptionId));

        Assert.False(result);

        repository.Verify(
            x => x.Remove(It.IsAny<Subscription>()),
            Times.Never);

        repository.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }
}
