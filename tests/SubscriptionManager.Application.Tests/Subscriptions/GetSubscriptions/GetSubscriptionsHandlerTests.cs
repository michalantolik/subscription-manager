using Moq;
using SubscriptionManager.Application.Subscriptions;
using SubscriptionManager.Application.Subscriptions.GetSubscriptions;
using SubscriptionManager.Domain.Subscriptions;

namespace SubscriptionManager.Application.Tests.Subscriptions.GetSubscriptions;

public sealed class GetSubscriptionsHandlerTests
{
    [Fact]
    public async Task HandleAsync_ShouldReturnMappedSubscriptions()
    {
        var subscriptions = new[]
        {
            new Subscription(
                Guid.NewGuid(),
                "Netflix",
                49m,
                "PLN",
                BillingPeriod.Monthly,
                new DateOnly(2026, 1, 1)),
            new Subscription(
                Guid.NewGuid(),
                "Microsoft 365",
                299m,
                "PLN",
                BillingPeriod.Yearly,
                new DateOnly(2026, 2, 1))
        };

        var repository = new Mock<ISubscriptionRepository>();

        repository
            .Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(subscriptions);

        var handler = new GetSubscriptionsHandler(repository.Object);

        var result = await handler.HandleAsync();

        Assert.Equal(2, result.Count);

        Assert.Collection(
            result,
            first =>
            {
                Assert.Equal("Netflix", first.Name);
                Assert.Equal(49m, first.Amount);
                Assert.Equal("PLN", first.Currency);
                Assert.Equal(BillingPeriod.Monthly, first.BillingPeriod);
            },
            second =>
            {
                Assert.Equal("Microsoft 365", second.Name);
                Assert.Equal(299m, second.Amount);
                Assert.Equal("PLN", second.Currency);
                Assert.Equal(BillingPeriod.Yearly, second.BillingPeriod);
            });
    }

    [Fact]
    public async Task HandleAsync_ShouldReturnEmptyCollection_WhenNoSubscriptionsExist()
    {
        var repository = new Mock<ISubscriptionRepository>();

        repository
            .Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<Subscription>());

        var handler = new GetSubscriptionsHandler(repository.Object);

        var result = await handler.HandleAsync();

        Assert.Empty(result);
    }
}
