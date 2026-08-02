using Moq;
using SubscriptionManager.Application.Common.Identity;
using SubscriptionManager.Application.Identity.UpdateBaseCurrency;
using SubscriptionManager.Domain.Subscriptions;

namespace SubscriptionManager.Application.Tests.Identity.UpdateBaseCurrency;

public sealed class UpdateBaseCurrencyHandlerTests
{
    [Fact]
    public async Task HandleAsync_ShouldUpdateBaseCurrency_WhenUserExists()
    {
        var userId = Guid.NewGuid();

        var identityService =
            new Mock<IIdentityService>();

        identityService
            .Setup(service =>
                service.UpdateBaseCurrencyAsync(
                    userId,
                    Currency.EUR,
                    It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var handler =
            new UpdateBaseCurrencyHandler(
                identityService.Object);

        var result =
            await handler.HandleAsync(
                new UpdateBaseCurrencyCommand(
                    userId,
                    Currency.EUR));

        Assert.True(result);

        identityService.Verify(
            service =>
                service.UpdateBaseCurrencyAsync(
                    userId,
                    Currency.EUR,
                    It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleAsync_ShouldReturnFalse_WhenUserDoesNotExist()
    {
        var userId = Guid.NewGuid();

        var identityService =
            new Mock<IIdentityService>();

        identityService
            .Setup(service =>
                service.UpdateBaseCurrencyAsync(
                    userId,
                    Currency.EUR,
                    It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var handler =
            new UpdateBaseCurrencyHandler(
                identityService.Object);

        var result =
            await handler.HandleAsync(
                new UpdateBaseCurrencyCommand(
                    userId,
                    Currency.EUR));

        Assert.False(result);

        identityService.Verify(
            service =>
                service.UpdateBaseCurrencyAsync(
                    userId,
                    Currency.EUR,
                    It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleAsync_ShouldThrow_WhenCommandIsNull()
    {
        var identityService =
            new Mock<IIdentityService>();

        var handler =
            new UpdateBaseCurrencyHandler(
                identityService.Object);

        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            handler.HandleAsync(null!));

        identityService.Verify(
            service =>
                service.UpdateBaseCurrencyAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<Currency>(),
                    It.IsAny<CancellationToken>()),
            Times.Never);
    }
}
