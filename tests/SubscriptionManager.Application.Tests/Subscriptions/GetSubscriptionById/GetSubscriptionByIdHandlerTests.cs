using Moq;
using SubscriptionManager.Application.Common.Authentication;
using SubscriptionManager.Application.Subscriptions;
using SubscriptionManager.Application.Subscriptions.GetSubscriptionById;
using SubscriptionManager.Domain.Subscriptions;

namespace SubscriptionManager.Application.Tests.Subscriptions.GetSubscriptionById;

public sealed class GetSubscriptionByIdHandlerTests
{
    [Fact]
    public async Task HandleAsync_ShouldReturnCurrentUserSubscription_WhenSubscriptionExists()
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

        var handler = new GetSubscriptionByIdHandler(
            repository.Object,
            currentUser.Object);

        var result = await handler.HandleAsync(subscriptionId);

        Assert.NotNull(result);
        Assert.Equal(subscriptionId, result.Id);
        Assert.Equal("Netflix", result.Name);
        Assert.Equal(49m, result.Amount);
        Assert.Equal("PLN", result.Currency);
        Assert.Equal(BillingPeriod.Monthly, result.BillingPeriod);

        repository.Verify(
            x => x.GetByIdAsync(
                subscriptionId,
                ownerId,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleAsync_ShouldReturnNull_WhenCurrentUserSubscriptionDoesNotExist()
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

        var handler = new GetSubscriptionByIdHandler(
            repository.Object,
            currentUser.Object);

        var result = await handler.HandleAsync(subscriptionId);

        Assert.Null(result);

        repository.Verify(
            x => x.GetByIdAsync(
                subscriptionId,
                ownerId,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
