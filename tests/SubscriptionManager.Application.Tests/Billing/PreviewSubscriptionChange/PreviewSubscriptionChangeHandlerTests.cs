using Moq;
using SubscriptionManager.Application.Billing;
using SubscriptionManager.Application.Billing.PaymentProvider;
using SubscriptionManager.Application.Billing.PreviewSubscriptionChange;
using SubscriptionManager.Application.Common.Identity;
using SubscriptionManager.Domain.Billing;

namespace SubscriptionManager.Application.Tests.Billing.PreviewSubscriptionChange;

public sealed class PreviewSubscriptionChangeHandlerTests
{
    private static readonly DateTimeOffset CurrentTime =
        new(
            2026,
            8,
            11,
            12,
            0,
            0,
            TimeSpan.Zero);

    [Fact]
    public async Task HandleAsync_ShouldReturnPreviewForImmediateUpgrade()
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

        var timeProvider =
            CreateTimeProvider();

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
                manager.PreviewChangeAsync(
                    "sub_123",
                    SubscriptionPlan.Premium,
                    BillingInterval.Monthly,
                    BillingSubscriptionChangeTiming.Immediate,
                    It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                new PaymentSubscriptionChangePreview(
                    25.50m,
                    "PLN",
                    CurrentTime));

        var handler =
            new PreviewSubscriptionChangeHandler(
                currentUser.Object,
                repository.Object,
                paymentSubscriptionManager.Object,
                timeProvider.Object);

        var command =
            new PreviewSubscriptionChangeCommand(
                SubscriptionPlan.Premium,
                BillingInterval.Monthly);

        var result =
            await handler.HandleAsync(
                command);

        Assert.Equal(
            SubscriptionPlan.Plus,
            result.CurrentPlan);

        Assert.Equal(
            BillingInterval.Monthly,
            result.CurrentBillingInterval);

        Assert.Equal(
            SubscriptionPlan.Premium,
            result.TargetPlan);

        Assert.Equal(
            BillingInterval.Monthly,
            result.TargetBillingInterval);

        Assert.Equal(
            BillingSubscriptionChangeTiming.Immediate,
            result.Timing);

        Assert.Equal(
            25.50m,
            result.AmountDueNow);

        Assert.Equal(
            "PLN",
            result.Currency);

        Assert.Equal(
            CurrentTime,
            result.EffectiveAt);

        paymentSubscriptionManager.Verify(
            manager =>
                manager.PreviewChangeAsync(
                    "sub_123",
                    SubscriptionPlan.Premium,
                    BillingInterval.Monthly,
                    BillingSubscriptionChangeTiming.Immediate,
                    It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleAsync_ShouldThrow_WhenPaidSubscriptionDoesNotExist()
    {
        var userId =
            Guid.NewGuid();

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
                (BillingSubscription?)null);

        var handler =
            new PreviewSubscriptionChangeHandler(
                currentUser.Object,
                repository.Object,
                paymentSubscriptionManager.Object,
                CreateTimeProvider().Object);

        var command =
            new PreviewSubscriptionChangeCommand(
                SubscriptionPlan.Premium,
                BillingInterval.Monthly);

        var exception =
            await Assert.ThrowsAsync<
                BillingSubscriptionChangeUnavailableException>(
                () =>
                    handler.HandleAsync(
                        command));

        Assert.Equal(
            "A paid billing subscription is required.",
            exception.Message);

        paymentSubscriptionManager.Verify(
            manager =>
                manager.PreviewChangeAsync(
                    It.IsAny<string>(),
                    It.IsAny<SubscriptionPlan>(),
                    It.IsAny<BillingInterval>(),
                    It.IsAny<BillingSubscriptionChangeTiming>(),
                    It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task HandleAsync_ShouldThrow_WhenSubscriptionIsNotActive()
    {
        var userId =
            Guid.NewGuid();

        var subscription =
            CreateSubscription(
                userId);

        subscription.Synchronize(
            SubscriptionPlan.Plus,
            BillingInterval.Monthly,
            BillingSubscriptionStatus.PastDue,
            "price_plus_monthly",
            CurrentTime.AddDays(-1),
            CurrentTime.AddMonths(1),
            false);

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
            new PreviewSubscriptionChangeHandler(
                currentUser.Object,
                repository.Object,
                paymentSubscriptionManager.Object,
                CreateTimeProvider().Object);

        var command =
            new PreviewSubscriptionChangeCommand(
                SubscriptionPlan.Premium,
                BillingInterval.Monthly);

        var exception =
            await Assert.ThrowsAsync<
                BillingSubscriptionChangeUnavailableException>(
                () =>
                    handler.HandleAsync(
                        command));

        Assert.Equal(
            "The billing subscription is not active.",
            exception.Message);

        paymentSubscriptionManager.Verify(
            manager =>
                manager.PreviewChangeAsync(
                    It.IsAny<string>(),
                    It.IsAny<SubscriptionPlan>(),
                    It.IsAny<BillingInterval>(),
                    It.IsAny<BillingSubscriptionChangeTiming>(),
                    It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task HandleAsync_ShouldThrow_WhenCancellationIsScheduled()
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

        var handler =
            new PreviewSubscriptionChangeHandler(
                currentUser.Object,
                repository.Object,
                paymentSubscriptionManager.Object,
                CreateTimeProvider().Object);

        var command =
            new PreviewSubscriptionChangeCommand(
                SubscriptionPlan.Premium,
                BillingInterval.Monthly);

        var exception =
            await Assert.ThrowsAsync<
                BillingSubscriptionChangeUnavailableException>(
                () =>
                    handler.HandleAsync(
                        command));

        Assert.Equal(
            "The scheduled cancellation must be resumed before changing the subscription.",
            exception.Message);

        paymentSubscriptionManager.Verify(
            manager =>
                manager.PreviewChangeAsync(
                    It.IsAny<string>(),
                    It.IsAny<SubscriptionPlan>(),
                    It.IsAny<BillingInterval>(),
                    It.IsAny<BillingSubscriptionChangeTiming>(),
                    It.IsAny<CancellationToken>()),
            Times.Never);
    }

    private static BillingSubscription CreateSubscription(
        Guid userId)
    {
        var subscription =
            new BillingSubscription(
                Guid.NewGuid(),
                userId,
                SubscriptionPlan.Plus,
                BillingInterval.Monthly,
                CurrentTime.AddDays(-1),
                CurrentTime.AddMonths(1));

        subscription.LinkToPaymentProvider(
            "cus_123",
            "sub_123",
            "price_plus_monthly");

        return subscription;
    }

    private static Mock<TimeProvider> CreateTimeProvider()
    {
        var timeProvider =
            new Mock<TimeProvider>();

        timeProvider
            .Setup(provider =>
                provider.GetUtcNow())
            .Returns(
                CurrentTime);

        return timeProvider;
    }
}
