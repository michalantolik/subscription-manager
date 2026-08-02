using Moq;
using SubscriptionManager.Application.Common.Identity;
using SubscriptionManager.Application.Subscriptions;
using SubscriptionManager.Application.Subscriptions.GetSubscriptions;
using SubscriptionManager.Domain.Subscriptions;

namespace SubscriptionManager.Application.Tests.Subscriptions.GetSubscriptions;

public sealed class GetSubscriptionsHandlerTests
{
    [Fact]
    public async Task HandleAsync_ShouldReturnCurrentUserSubscriptions()
    {
        var ownerId = Guid.NewGuid();

        var subscriptions = new[]
        {
            new Subscription(
                Guid.NewGuid(),
                ownerId,
                "Netflix",
                49m,
                Currency.PLN,
                BillingPeriod.Monthly,
                new DateOnly(2026, 1, 1)),

            new Subscription(
                Guid.NewGuid(),
                ownerId,
                "Microsoft 365",
                299m,
                Currency.PLN,
                BillingPeriod.Yearly,
                new DateOnly(2026, 2, 1))
        };

        var repository =
            new Mock<ISubscriptionRepository>();

        var currentUser = new Mock<ICurrentUser>();

        currentUser
            .SetupGet(x => x.UserId)
            .Returns(ownerId);

        repository
            .Setup(x => x.GetAllAsync(
                ownerId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(subscriptions);

        var handler = new GetSubscriptionsHandler(
            repository.Object,
            currentUser.Object);

        var result = await handler.HandleAsync();

        Assert.Equal(2, result.Count);

        Assert.Collection(
            result,
            first =>
            {
                Assert.Equal(
                    "Netflix",
                    first.Name);

                Assert.Equal(
                    49m,
                    first.Amount);

                Assert.Equal(
                    Currency.PLN,
                    first.Currency);

                Assert.Equal(
                    BillingPeriod.Monthly,
                    first.BillingPeriod);
            },
            second =>
            {
                Assert.Equal(
                    "Microsoft 365",
                    second.Name);

                Assert.Equal(
                    299m,
                    second.Amount);

                Assert.Equal(
                    Currency.PLN,
                    second.Currency);

                Assert.Equal(
                    BillingPeriod.Yearly,
                    second.BillingPeriod);
            });

        repository.Verify(
            x => x.GetAllAsync(
                ownerId,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleAsync_ShouldReturnEmptyCollection_WhenCurrentUserHasNoSubscriptions()
    {
        var ownerId = Guid.NewGuid();

        var repository =
            new Mock<ISubscriptionRepository>();

        var currentUser = new Mock<ICurrentUser>();

        currentUser
            .SetupGet(x => x.UserId)
            .Returns(ownerId);

        repository
            .Setup(x => x.GetAllAsync(
                ownerId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                Array.Empty<Subscription>());

        var handler = new GetSubscriptionsHandler(
            repository.Object,
            currentUser.Object);

        var result = await handler.HandleAsync();

        Assert.Empty(result);

        repository.Verify(
            x => x.GetAllAsync(
                ownerId,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
