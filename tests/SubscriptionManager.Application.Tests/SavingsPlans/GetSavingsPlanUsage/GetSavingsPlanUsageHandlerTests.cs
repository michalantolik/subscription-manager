using Moq;
using SubscriptionManager.Application.Common.Identity;
using SubscriptionManager.Application.SavingsPlans;
using SubscriptionManager.Application.SavingsPlans.GetSavingsPlanUsage;
using SubscriptionManager.Domain.Billing;

namespace SubscriptionManager.Application.Tests.SavingsPlans.GetSavingsPlanUsage;

public sealed class GetSavingsPlanUsageHandlerTests
{
    [Fact]
    public async Task HandleAsync_ShouldReturnZeroUsageForFreePlan()
    {
        var userId =
            Guid.NewGuid();

        var identityService =
            new Mock<IIdentityService>();

        var currentUser =
            new Mock<ICurrentUser>();

        var usageRepository =
            new Mock<ISavingsPlanUsageRepository>();

        currentUser
            .SetupGet(user =>
                user.UserId)
            .Returns(userId);

        identityService
            .Setup(service =>
                service.GetSubscriptionPlanAsync(
                    userId,
                    It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                SubscriptionPlan.Free);

        var handler =
            new GetSavingsPlanUsageHandler(
                identityService.Object,
                currentUser.Object,
                usageRepository.Object);

        var result =
            await handler.HandleAsync();

        Assert.Equal(
            SubscriptionPlan.Free,
            result.SubscriptionPlan);

        Assert.Equal(
            0,
            result.DailyRequestLimit);

        Assert.Equal(
            0,
            result.RemainingRequestCount);

        identityService.Verify(
            service =>
                service.GetSubscriptionPlanAsync(
                    userId,
                    It.IsAny<CancellationToken>()),
            Times.Once);

        usageRepository.Verify(
            repository =>
                repository.GetRemainingRequestCountAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<DateOnly>(),
                    It.IsAny<int>(),
                    It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task HandleAsync_ShouldReturnCurrentUsageForPlusPlan()
    {
        var userId =
            Guid.NewGuid();

        var identityService =
            new Mock<IIdentityService>();

        var currentUser =
            new Mock<ICurrentUser>();

        var usageRepository =
            new Mock<ISavingsPlanUsageRepository>();

        currentUser
            .SetupGet(user =>
                user.UserId)
            .Returns(userId);

        identityService
            .Setup(service =>
                service.GetSubscriptionPlanAsync(
                    userId,
                    It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                SubscriptionPlan.Plus);

        usageRepository
            .Setup(repository =>
                repository.GetRemainingRequestCountAsync(
                    userId,
                    It.IsAny<DateOnly>(),
                    SubscriptionPlanLimits
                        .PlusDailySavingsPlanLimit,
                    It.IsAny<CancellationToken>()))
            .ReturnsAsync(3);

        var handler =
            new GetSavingsPlanUsageHandler(
                identityService.Object,
                currentUser.Object,
                usageRepository.Object);

        var result =
            await handler.HandleAsync();

        Assert.Equal(
            SubscriptionPlan.Plus,
            result.SubscriptionPlan);

        Assert.Equal(
            SubscriptionPlanLimits
                .PlusDailySavingsPlanLimit,
            result.DailyRequestLimit);

        Assert.Equal(
            3,
            result.RemainingRequestCount);

        usageRepository.Verify(
            repository =>
                repository.GetRemainingRequestCountAsync(
                    userId,
                    It.IsAny<DateOnly>(),
                    SubscriptionPlanLimits
                        .PlusDailySavingsPlanLimit,
                    It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleAsync_ShouldReturnCurrentUsageForPremiumPlan()
    {
        var userId =
            Guid.NewGuid();

        var identityService =
            new Mock<IIdentityService>();

        var currentUser =
            new Mock<ICurrentUser>();

        var usageRepository =
            new Mock<ISavingsPlanUsageRepository>();

        currentUser
            .SetupGet(user =>
                user.UserId)
            .Returns(userId);

        identityService
            .Setup(service =>
                service.GetSubscriptionPlanAsync(
                    userId,
                    It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                SubscriptionPlan.Premium);

        usageRepository
            .Setup(repository =>
                repository.GetRemainingRequestCountAsync(
                    userId,
                    It.IsAny<DateOnly>(),
                    SubscriptionPlanLimits
                        .PremiumDailySavingsPlanLimit,
                    It.IsAny<CancellationToken>()))
            .ReturnsAsync(12);

        var handler =
            new GetSavingsPlanUsageHandler(
                identityService.Object,
                currentUser.Object,
                usageRepository.Object);

        var result =
            await handler.HandleAsync();

        Assert.Equal(
            SubscriptionPlan.Premium,
            result.SubscriptionPlan);

        Assert.Equal(
            SubscriptionPlanLimits
                .PremiumDailySavingsPlanLimit,
            result.DailyRequestLimit);

        Assert.Equal(
            12,
            result.RemainingRequestCount);

        usageRepository.Verify(
            repository =>
                repository.GetRemainingRequestCountAsync(
                    userId,
                    It.IsAny<DateOnly>(),
                    SubscriptionPlanLimits
                        .PremiumDailySavingsPlanLimit,
                    It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleAsync_ShouldThrow_WhenSubscriptionPlanIsUnavailable()
    {
        var userId =
            Guid.NewGuid();

        var identityService =
            new Mock<IIdentityService>();

        var currentUser =
            new Mock<ICurrentUser>();

        var usageRepository =
            new Mock<ISavingsPlanUsageRepository>();

        currentUser
            .SetupGet(user =>
                user.UserId)
            .Returns(userId);

        identityService
            .Setup(service =>
                service.GetSubscriptionPlanAsync(
                    userId,
                    It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                (SubscriptionPlan?)null);

        var handler =
            new GetSavingsPlanUsageHandler(
                identityService.Object,
                currentUser.Object,
                usageRepository.Object);

        var exception =
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => handler.HandleAsync());

        Assert.Equal(
            "The current user's subscription plan is unavailable.",
            exception.Message);

        usageRepository.Verify(
            repository =>
                repository.GetRemainingRequestCountAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<DateOnly>(),
                    It.IsAny<int>(),
                    It.IsAny<CancellationToken>()),
            Times.Never);
    }
}
