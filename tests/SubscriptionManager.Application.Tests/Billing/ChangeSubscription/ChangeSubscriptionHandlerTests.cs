using Moq;
using SubscriptionManager.Application.Billing;
using SubscriptionManager.Application.Billing.ChangeSubscription;
using SubscriptionManager.Application.Billing.GetBillingOverview;
using SubscriptionManager.Application.Billing.PreviewSubscriptionChange;
using SubscriptionManager.Application.Common.Identity;
using SubscriptionManager.Domain.Billing;

namespace SubscriptionManager.Application.Tests.Billing.ChangeSubscription;

public sealed class ChangeSubscriptionHandlerTests
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
    public async Task HandleAsync_ShouldExecuteImmediateUpgrade()
    {
        var userId =
            Guid.NewGuid();

        var subscription =
            CreateSubscription(
                userId,
                SubscriptionPlan.Plus);

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
                manager.ChangeAsync(
                    "sub_123",
                    SubscriptionPlan.Premium,
                    BillingInterval.Monthly,
                    BillingSubscriptionChangeTiming.Immediate,
                    It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                new PaymentSubscriptionChangeResult(
                    CreateProviderState(
                        SubscriptionPlan.Premium,
                        BillingInterval.Monthly)));

        var handler =
            new ChangeSubscriptionHandler(
                currentUser.Object,
                repository.Object,
                paymentSubscriptionManager.Object,
                CreateTimeProvider().Object);

        var command =
            new ChangeSubscriptionCommand(
                SubscriptionPlan.Premium,
                BillingInterval.Monthly);

        await handler.HandleAsync(
            command);

        paymentSubscriptionManager.Verify(
            manager =>
                manager.ChangeAsync(
                    "sub_123",
                    SubscriptionPlan.Premium,
                    BillingInterval.Monthly,
                    BillingSubscriptionChangeTiming.Immediate,
                    It.IsAny<CancellationToken>()),
            Times.Once);

        repository.Verify(
            currentRepository =>
                currentRepository.SaveChangesAsync(
                    It.IsAny<CancellationToken>()),
            Times.Once);

        Assert.Equal(
            SubscriptionPlan.Premium,
            subscription.Plan);

        Assert.Equal(
            BillingInterval.Monthly,
            subscription.BillingInterval);

        var overview =
            await new GetBillingOverviewHandler(
                    repository.Object,
                    currentUser.Object)
                .HandleAsync();

        Assert.Equal(
            SubscriptionPlan.Premium,
            overview.Plan);
    }

    [Fact]
    public async Task HandleAsync_ShouldScheduleDowngradeForNextBillingPeriod()
    {
        var userId =
            Guid.NewGuid();

        var subscription =
            CreateSubscription(
                userId,
                SubscriptionPlan.Premium);

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
                manager.ChangeAsync(
                    "sub_123",
                    SubscriptionPlan.Plus,
                    BillingInterval.Monthly,
                    BillingSubscriptionChangeTiming.NextBillingPeriod,
                    It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                new PaymentSubscriptionChangeResult(
                    UpdatedSubscription: null));

        var handler =
            new ChangeSubscriptionHandler(
                currentUser.Object,
                repository.Object,
                paymentSubscriptionManager.Object,
                CreateTimeProvider().Object);

        var command =
            new ChangeSubscriptionCommand(
                SubscriptionPlan.Plus,
                BillingInterval.Monthly);

        await handler.HandleAsync(
            command);

        paymentSubscriptionManager.Verify(
            manager =>
                manager.ChangeAsync(
                    "sub_123",
                    SubscriptionPlan.Plus,
                    BillingInterval.Monthly,
                    BillingSubscriptionChangeTiming
                        .NextBillingPeriod,
                    It.IsAny<CancellationToken>()),
            Times.Once);

        repository.Verify(
            currentRepository =>
                currentRepository.SaveChangesAsync(
                    It.IsAny<CancellationToken>()),
            Times.Never);

        Assert.Equal(
            SubscriptionPlan.Premium,
            subscription.Plan);
    }

    [Fact]
    public async Task HandleAsync_ShouldNotChangeLocalState_WhenPaymentProviderFails()
    {
        var userId =
            Guid.NewGuid();

        var subscription =
            CreateSubscription(
                userId,
                SubscriptionPlan.Plus);

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
                manager.ChangeAsync(
                    "sub_123",
                    SubscriptionPlan.Premium,
                    BillingInterval.Monthly,
                    BillingSubscriptionChangeTiming.Immediate,
                    It.IsAny<CancellationToken>()))
            .ThrowsAsync(
                new HttpRequestException(
                    "Payment provider unavailable."));

        var handler =
            new ChangeSubscriptionHandler(
                currentUser.Object,
                repository.Object,
                paymentSubscriptionManager.Object,
                CreateTimeProvider().Object);

        await Assert.ThrowsAsync<HttpRequestException>(
            () =>
                handler.HandleAsync(
                    new ChangeSubscriptionCommand(
                        SubscriptionPlan.Premium,
                        BillingInterval.Monthly)));

        repository.Verify(
            currentRepository =>
                currentRepository.SaveChangesAsync(
                    It.IsAny<CancellationToken>()),
            Times.Never);

        Assert.Equal(
            SubscriptionPlan.Plus,
            subscription.Plan);
    }

    [Fact]
    public async Task HandleAsync_ShouldNotCallPaymentProvider_WhenCancellationIsScheduled()
    {
        var userId =
            Guid.NewGuid();

        var subscription =
            CreateSubscription(
                userId,
                SubscriptionPlan.Plus);

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
            new ChangeSubscriptionHandler(
                currentUser.Object,
                repository.Object,
                paymentSubscriptionManager.Object,
                CreateTimeProvider().Object);

        var command =
            new ChangeSubscriptionCommand(
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
                manager.ChangeAsync(
                    It.IsAny<string>(),
                    It.IsAny<SubscriptionPlan>(),
                    It.IsAny<BillingInterval>(),
                    It.IsAny<BillingSubscriptionChangeTiming>(),
                    It.IsAny<CancellationToken>()),
            Times.Never);

        repository.Verify(
            currentRepository =>
                currentRepository.SaveChangesAsync(
                    It.IsAny<CancellationToken>()),
            Times.Never);
    }

    private static BillingSubscription CreateSubscription(
        Guid userId,
        SubscriptionPlan plan)
    {
        var subscription =
            new BillingSubscription(
                Guid.NewGuid(),
                userId,
                plan,
                BillingInterval.Monthly,
                CurrentTime.AddDays(-1),
                CurrentTime.AddMonths(1));

        subscription.LinkToPaymentProvider(
            "cus_123",
            "sub_123",
            plan == SubscriptionPlan.Plus
                ? "price_plus_monthly"
                : "price_premium_monthly");

        return subscription;
    }


    private static PaymentSubscriptionState CreateProviderState(
        SubscriptionPlan plan,
        BillingInterval billingInterval)
    {
        return new PaymentSubscriptionState(
            plan,
            billingInterval,
            BillingSubscriptionStatus.Active,
            plan == SubscriptionPlan.Plus
                ? "price_plus_monthly"
                : "price_premium_monthly",
            CurrentTime.AddDays(-1),
            CurrentTime.AddMonths(1),
            CancelAtPeriodEnd: false);
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
