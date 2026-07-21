using Moq;
using SubscriptionManager.Application.Subscriptions;
using SubscriptionManager.Application.Subscriptions.GetSubscriptionById;
using SubscriptionManager.Domain.Subscriptions;

namespace SubscriptionManager.Application.Tests.Subscriptions.GetSubscriptionById;

public sealed class GetSubscriptionByIdHandlerTests
{
    [Fact]
    public async Task HandleAsync_ShouldReturnSubscription_WhenSubscriptionExists()
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

        var handler = new GetSubscriptionByIdHandler(repository.Object);

        var result = await handler.HandleAsync(subscriptionId);

        Assert.NotNull(result);
        Assert.Equal(subscriptionId, result.Id);
        Assert.Equal("Netflix", result.Name);
        Assert.Equal(49m, result.Amount);
        Assert.Equal("PLN", result.Currency);
        Assert.Equal(BillingPeriod.Monthly, result.BillingPeriod);
    }

    [Fact]
    public async Task HandleAsync_ShouldReturnNull_WhenSubscriptionDoesNotExist()
    {
        var subscriptionId = Guid.NewGuid();

        var repository = new Mock<ISubscriptionRepository>();

        repository
            .Setup(x => x.GetByIdAsync(
                subscriptionId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Subscription?)null);

        var handler = new GetSubscriptionByIdHandler(repository.Object);

        var result = await handler.HandleAsync(subscriptionId);

        Assert.Null(result);
    }
}
