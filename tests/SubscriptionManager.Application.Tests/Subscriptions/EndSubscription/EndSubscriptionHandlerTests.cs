using Moq;
using SubscriptionManager.Application.Subscriptions;
using SubscriptionManager.Application.Subscriptions.EndSubscription;
using SubscriptionManager.Domain.Subscriptions;

namespace SubscriptionManager.Application.Tests.Subscriptions.EndSubscription;

public sealed class EndSubscriptionHandlerTests
{
    [Fact]
    public async Task HandleAsync_ShouldEndSubscription_WhenSubscriptionExists()
    {
        var subscriptionId = Guid.NewGuid();
        var endDate = new DateOnly(2026, 7, 21);

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

        var handler = new EndSubscriptionHandler(repository.Object);

        var result = await handler.HandleAsync(
            new EndSubscriptionCommand(subscriptionId, endDate));

        Assert.True(result);
        Assert.False(subscription.IsActive);
        Assert.Equal(endDate, subscription.EndDate);

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

        var handler = new EndSubscriptionHandler(repository.Object);

        var result = await handler.HandleAsync(
            new EndSubscriptionCommand(
                subscriptionId,
                new DateOnly(2026, 7, 21)));

        Assert.False(result);

        repository.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }
}
