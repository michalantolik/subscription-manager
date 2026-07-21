using Moq;
using SubscriptionManager.Application.Subscriptions;
using SubscriptionManager.Application.Subscriptions.CreateSubscription;
using SubscriptionManager.Domain.Subscriptions;

namespace SubscriptionManager.Application.Tests.Subscriptions.CreateSubscription;

public sealed class CreateSubscriptionHandlerTests
{
    [Fact]
    public async Task HandleAsync_ShouldCreateSubscription()
    {
        var repository = new Mock<ISubscriptionRepository>();

        Subscription? addedSubscription = null;

        repository
            .Setup(x => x.AddAsync(
                It.IsAny<Subscription>(),
                It.IsAny<CancellationToken>()))
            .Callback<Subscription, CancellationToken>(
                (subscription, _) => addedSubscription = subscription)
            .Returns(Task.CompletedTask);

        var handler = new CreateSubscriptionHandler(repository.Object);

        var result = await handler.HandleAsync(
            new CreateSubscriptionCommand(
                "  Netflix  ",
                49m,
                "pln",
                BillingPeriod.Monthly,
                new DateOnly(2026, 1, 1)));

        Assert.NotEqual(Guid.Empty, result);
        Assert.NotNull(addedSubscription);
        Assert.Equal(result, addedSubscription.Id);
        Assert.Equal("Netflix", addedSubscription.Name);
        Assert.Equal(49m, addedSubscription.Amount);
        Assert.Equal("PLN", addedSubscription.Currency);
        Assert.Equal(BillingPeriod.Monthly, addedSubscription.BillingPeriod);
        Assert.Equal(new DateOnly(2026, 1, 1), addedSubscription.StartDate);

        repository.Verify(
            x => x.AddAsync(
                addedSubscription,
                It.IsAny<CancellationToken>()),
            Times.Once);

        repository.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleAsync_ShouldThrow_WhenCommandIsNull()
    {
        var repository = new Mock<ISubscriptionRepository>();

        var handler = new CreateSubscriptionHandler(repository.Object);

        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            handler.HandleAsync(null!));

        repository.Verify(
            x => x.AddAsync(
                It.IsAny<Subscription>(),
                It.IsAny<CancellationToken>()),
            Times.Never);

        repository.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }
}
