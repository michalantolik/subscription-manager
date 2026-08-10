using Moq;
using SubscriptionManager.Application.Billing;
using SubscriptionManager.Application.Billing.GetBillingOverview;
using SubscriptionManager.Application.Common.Identity;
using SubscriptionManager.Domain.Billing;

namespace SubscriptionManager.Application.Tests.Billing.GetBillingOverview;

public sealed class GetBillingOverviewHandlerTests
{
    [Fact]
    public async Task HandleAsync_ShouldReturnFreePlan_WhenSubscriptionDoesNotExist()
    {
        var userId =
            Guid.NewGuid();

        var currentUser =
            new Mock<ICurrentUser>();

        var billingSubscriptionRepository =
            new Mock<IBillingSubscriptionRepository>();

        currentUser
            .SetupGet(user =>
                user.UserId)
            .Returns(userId);

        billingSubscriptionRepository
            .Setup(repository =>
                repository.GetByUserIdAsync(
                    userId,
                    It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                (BillingSubscription?)null);

        var handler =
            new GetBillingOverviewHandler(
                billingSubscriptionRepository.Object,
                currentUser.Object);

        var result =
            await handler.HandleAsync();

        Assert.Equal(
            SubscriptionPlan.Free,
            result.Plan);

        Assert.Null(
            result.BillingInterval);

        Assert.Null(
            result.Status);

        Assert.Null(
            result.CurrentPeriodStart);

        Assert.Null(
            result.CurrentPeriodEnd);

        Assert.False(
            result.CancelAtPeriodEnd);
    }

    [Fact]
    public async Task HandleAsync_ShouldReturnCurrentBillingSubscription()
    {
        var userId =
            Guid.NewGuid();

        var periodStart =
            new DateTimeOffset(
                2026,
                8,
                10,
                0,
                0,
                0,
                TimeSpan.Zero);

        var periodEnd =
            periodStart.AddMonths(1);

        var subscription =
            new BillingSubscription(
                Guid.NewGuid(),
                userId,
                SubscriptionPlan.Plus,
                BillingInterval.Monthly,
                periodStart,
                periodEnd);

        subscription.Synchronize(
            SubscriptionPlan.Plus,
            BillingInterval.Monthly,
            BillingSubscriptionStatus.Active,
            "price_plus_monthly",
            periodStart,
            periodEnd,
            true);

        var currentUser =
            new Mock<ICurrentUser>();

        var billingSubscriptionRepository =
            new Mock<IBillingSubscriptionRepository>();

        currentUser
            .SetupGet(user =>
                user.UserId)
            .Returns(userId);

        billingSubscriptionRepository
            .Setup(repository =>
                repository.GetByUserIdAsync(
                    userId,
                    It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                subscription);

        var handler =
            new GetBillingOverviewHandler(
                billingSubscriptionRepository.Object,
                currentUser.Object);

        var result =
            await handler.HandleAsync();

        Assert.Equal(
            SubscriptionPlan.Plus,
            result.Plan);

        Assert.Equal(
            BillingInterval.Monthly,
            result.BillingInterval);

        Assert.Equal(
            BillingSubscriptionStatus.Active,
            result.Status);

        Assert.Equal(
            periodStart,
            result.CurrentPeriodStart);

        Assert.Equal(
            periodEnd,
            result.CurrentPeriodEnd);

        Assert.True(
            result.CancelAtPeriodEnd);

        billingSubscriptionRepository.Verify(
            repository =>
                repository.GetByUserIdAsync(
                    userId,
                    It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
