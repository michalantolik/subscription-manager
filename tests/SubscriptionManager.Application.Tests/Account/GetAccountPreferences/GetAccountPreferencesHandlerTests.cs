using Moq;
using SubscriptionManager.Application.Account;
using SubscriptionManager.Application.Account.GetAccountPreferences;
using SubscriptionManager.Application.Common.Identity;
using SubscriptionManager.Application.Common.Localization;
using SubscriptionManager.Domain.Subscriptions;

namespace SubscriptionManager.Application.Tests.Account.GetAccountPreferences;

public sealed class GetAccountPreferencesHandlerTests
{
    [Fact]
    public async Task HandleAsync_ShouldReturnAccountPreferences()
    {
        var userId = Guid.NewGuid();

        var expectedPreferences =
            new AccountPreferences(
                Language.English,
                Currency.USD);

        var identityService =
            new Mock<IIdentityService>();

        identityService
            .Setup(service =>
                service.GetAccountPreferencesAsync(
                    userId,
                    It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                expectedPreferences);

        var handler =
            new GetAccountPreferencesHandler(
                identityService.Object);

        var result =
            await handler.HandleAsync(userId);

        Assert.NotNull(result);
        Assert.Equal(
            Language.English,
            result.Language);
        Assert.Equal(
            Currency.USD,
            result.BaseCurrency);

        identityService.Verify(
            service =>
                service.GetAccountPreferencesAsync(
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
                service.GetAccountPreferencesAsync(
                    userId,
                    It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                (AccountPreferences?)null);

        var handler =
            new GetAccountPreferencesHandler(
                identityService.Object);

        var result =
            await handler.HandleAsync(userId);

        Assert.Null(result);

        identityService.Verify(
            service =>
                service.GetAccountPreferencesAsync(
                    userId,
                    It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
