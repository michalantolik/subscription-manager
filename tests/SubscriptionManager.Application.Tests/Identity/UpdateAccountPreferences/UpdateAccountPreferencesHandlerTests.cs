using Moq;
using SubscriptionManager.Application.Account.UpdateAccountPreferences;
using SubscriptionManager.Application.Common.Identity;
using SubscriptionManager.Application.Common.Localization;
using SubscriptionManager.Domain.Subscriptions;

namespace SubscriptionManager.Application.Tests.Identity.UpdateAccountPreferences;

public sealed class UpdateAccountPreferencesHandlerTests
{
    [Fact]
    public async Task HandleAsync_ShouldUpdateAccountPreferences()
    {
        var userId = Guid.NewGuid();

        var identityService =
            new Mock<IIdentityService>();

        identityService
            .Setup(service =>
                service.UpdateAccountPreferencesAsync(
                    userId,
                    Language.German,
                    Currency.EUR,
                    It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var handler =
            new UpdateAccountPreferencesHandler(
                identityService.Object);

        var command =
            new UpdateAccountPreferencesCommand(
                userId,
                Language.German,
                Currency.EUR);

        var result =
            await handler.HandleAsync(command);

        Assert.True(result);

        identityService.Verify(
            service =>
                service.UpdateAccountPreferencesAsync(
                    userId,
                    Language.German,
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
                service.UpdateAccountPreferencesAsync(
                    userId,
                    Language.English,
                    Currency.USD,
                    It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var handler =
            new UpdateAccountPreferencesHandler(
                identityService.Object);

        var command =
            new UpdateAccountPreferencesCommand(
                userId,
                Language.English,
                Currency.USD);

        var result =
            await handler.HandleAsync(command);

        Assert.False(result);

        identityService.Verify(
            service =>
                service.UpdateAccountPreferencesAsync(
                    userId,
                    Language.English,
                    Currency.USD,
                    It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
