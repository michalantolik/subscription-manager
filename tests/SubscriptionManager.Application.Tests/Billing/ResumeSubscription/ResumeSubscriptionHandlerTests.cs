using Moq;
using SubscriptionManager.Application.Billing;
using SubscriptionManager.Application.Billing.ResumeSubscription;
using SubscriptionManager.Application.Common.Identity;
using SubscriptionManager.Domain.Billing;

namespace SubscriptionManager.Application.Tests.Billing.ResumeSubscription;

public sealed class ResumeSubscriptionHandlerTests
{
    [Fact]
    public async Task HandleAsync_ShouldResumeSubscriptionWithPaymentProvider()
    {
        var userId =
            Guid.NewGuid();

        var subscription =
            CreateSubscription(
                userId);

        subscription.ScheduleCancellation();

        var currentUser =
            new Mock<ICurrentUser>();

        var repository =
            new Mock<IBillingSubscriptionRepository>();

        var paymentSubscriptionManager =
            new Mock<IPaymentSubscriptionManager>();

        currentUser
            .SetupGet(user =>
                user.UserId)
            .Returns(
                userId);

        repository
            .Setup(currentRepository =>
                currentRepository.GetByUserIdAsync(
                    userId,
                    It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                subscription);

        paymentSubscriptionManager
            .Setup(manager =>
                manager.ResumeAsync(
                    "sub_123",
                    It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                new PaymentSubscriptionState(
                    SubscriptionPlan.Plus,
                    BillingInterval.Monthly,
                    BillingSubscriptionStatus.Active,
                    "price_plus_monthly",
                    subscription.CurrentPeriodStart,
                    subscription.CurrentPeriodEnd,
                    CancelAtPeriodEnd: false));

        var handler =
            new ResumeSubscriptionHandler(
                currentUser.Object,
                repository.Object,
                paymentSubscriptionManager.Object);

        await handler.HandleAsync(
            new ResumeSubscriptionCommand());

        paymentSubscriptionManager.Verify(
            manager =>
                manager.ResumeAsync(
                    "sub_123",
                    It.IsAny<CancellationToken>()),
            Times.Once);

        repository.Verify(
            currentRepository =>
                currentRepository.SaveChangesAsync(
                    It.IsAny<CancellationToken>()),
            Times.Once);

        Assert.False(
            subscription.CancelAtPeriodEnd);
    }

    [Fact]
    public async Task HandleAsync_ShouldNotCallPaymentProvider_WhenRenewalIsAlreadyActive()
    {
        var userId =
            Guid.NewGuid();

        var subscription =
            CreateSubscription(
                userId);

        var currentUser =
            new Mock<ICurrentUser>();

        var repository =
            new Mock<IBillingSubscriptionRepository>();

        var paymentSubscriptionManager =
            new Mock<IPaymentSubscriptionManager>();

        currentUser
            .SetupGet(user =>
                user.UserId)
            .Returns(
                userId);

        repository
            .Setup(currentRepository =>
                currentRepository.GetByUserIdAsync(
                    userId,
                    It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                subscription);

        var handler =
            new ResumeSubscriptionHandler(
                currentUser.Object,
                repository.Object,
                paymentSubscriptionManager.Object);

        await handler.HandleAsync(
            new ResumeSubscriptionCommand());

        paymentSubscriptionManager.Verify(
            manager =>
                manager.ResumeAsync(
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()),
            Times.Never);

        repository.Verify(
            currentRepository =>
                currentRepository.SaveChangesAsync(
                    It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task HandleAsync_ShouldThrow_WhenSubscriptionHasEnded()
    {
        var userId =
            Guid.NewGuid();

        var subscription =
            CreateSubscription(
                userId);

        subscription.Synchronize(
            SubscriptionPlan.Plus,
            BillingInterval.Monthly,
            BillingSubscriptionStatus.Canceled,
            "price_plus_monthly",
            subscription.CurrentPeriodStart,
            subscription.CurrentPeriodEnd,
            true);

        var currentUser =
            new Mock<ICurrentUser>();

        var repository =
            new Mock<IBillingSubscriptionRepository>();

        var paymentSubscriptionManager =
            new Mock<IPaymentSubscriptionManager>();

        currentUser
            .SetupGet(user =>
                user.UserId)
            .Returns(
                userId);

        repository
            .Setup(currentRepository =>
                currentRepository.GetByUserIdAsync(
                    userId,
                    It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                subscription);

        var handler =
            new ResumeSubscriptionHandler(
                currentUser.Object,
                repository.Object,
                paymentSubscriptionManager.Object);

        var exception =
            await Assert.ThrowsAsync<
                BillingSubscriptionResumeUnavailableException>(
                () =>
                    handler.HandleAsync(
                        new ResumeSubscriptionCommand()));

        Assert.Equal(
            "The billing subscription has already ended.",
            exception.Message);

        paymentSubscriptionManager.Verify(
            manager =>
                manager.ResumeAsync(
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()),
            Times.Never);
    }

    private static BillingSubscription CreateSubscription(
        Guid userId)
    {
        var periodStart =
            new DateTimeOffset(
                2026,
                8,
                11,
                0,
                0,
                0,
                TimeSpan.Zero);

        var subscription =
            new BillingSubscription(
                Guid.NewGuid(),
                userId,
                SubscriptionPlan.Plus,
                BillingInterval.Monthly,
                periodStart,
                periodStart.AddMonths(1));

        subscription.LinkToPaymentProvider(
            "cus_123",
            "sub_123",
            "price_plus_monthly");

        return subscription;
    }
}
