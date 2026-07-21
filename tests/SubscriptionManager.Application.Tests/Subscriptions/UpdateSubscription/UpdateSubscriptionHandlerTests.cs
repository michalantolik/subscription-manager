using Moq;
using SubscriptionManager.Application.Subscriptions;
using SubscriptionManager.Application.Subscriptions.UpdateSubscription;
using SubscriptionManager.Domain.Subscriptions;

namespace SubscriptionManager.Application.Tests.Subscriptions.UpdateSubscription;

public sealed class UpdateSubscriptionHandlerTests
{
    [Fact]
    public async Task HandleAsync_ShouldUpdateSubscription_WhenSubscriptionExists()
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

        var handler = new UpdateSubscriptionHandler(repository.Object);

        var result = await handler.HandleAsync(
            new UpdateSubscriptionCommand(
                subscriptionId,
                "Spotify",
                59m,
                "eur",
                BillingPeriod.Yearly));

        Assert.True(result);
        Assert.Equal("Spotify", subscription.Name);
        Assert.Equal(59m, subscription.Amount);
        Assert.Equal("EUR", subscription.Currency);
        Assert.Equal(BillingPeriod.Yearly, subscription.BillingPeriod);

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

        var handler = new UpdateSubscriptionHandler(repository.Object);

        var result = await handler.HandleAsync(
            new UpdateSubscriptionCommand(
                subscriptionId,
                "Spotify",
                59m,
                "EUR",
                BillingPeriod.Yearly));

        Assert.False(result);

        repository.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }
}
