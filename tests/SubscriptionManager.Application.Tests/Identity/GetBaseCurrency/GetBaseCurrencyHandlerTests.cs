using Moq;
using SubscriptionManager.Application.Common.Identity;
using SubscriptionManager.Application.Identity.GetBaseCurrency;
using SubscriptionManager.Domain.Subscriptions;

namespace SubscriptionManager.Application.Tests.Identity.GetBaseCurrency;

public sealed class GetBaseCurrencyHandlerTests
{
    [Fact]
    public async Task HandleAsync_ShouldReturnBaseCurrency_WhenUserExists()
    {
        var userId = Guid.NewGuid();

        var identityService =
            new Mock<IIdentityService>();

        identityService
            .Setup(service =>
                service.GetBaseCurrencyAsync(
                    userId,
                    It.IsAny<CancellationToken>()))
            .ReturnsAsync(Currency.EUR);

        var handler =
            new GetBaseCurrencyHandler(
                identityService.Object);

        var result =
            await handler.HandleAsync(userId);

        Assert.Equal(
            Currency.EUR,
            result);

        identityService.Verify(
            service =>
                service.GetBaseCurrencyAsync(
                    userId,
                    It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleAsync_ShouldReturnNull_WhenUserDoesNotExist()
    {
        var userId = Guid.NewGuid();

        var identityService =
            new Mock<IIdentityService>();

        identityService
            .Setup(service =>
                service.GetBaseCurrencyAsync(
                    userId,
                    It.IsAny<CancellationToken>()))
            .ReturnsAsync((Currency?)null);

        var handler =
            new GetBaseCurrencyHandler(
                identityService.Object);

        var result =
            await handler.HandleAsync(userId);

        Assert.Null(result);

        identityService.Verify(
            service =>
                service.GetBaseCurrencyAsync(
                    userId,
                    It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
