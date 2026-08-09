using Moq;
using SubscriptionManager.Application.Common.Authentication;
using SubscriptionManager.Application.Common.Identity;
using SubscriptionManager.Application.Common.Localization;
using SubscriptionManager.Application.Identity.LoginUser;
using SubscriptionManager.Domain.Billing;

namespace SubscriptionManager.Application.Tests.Identity.LoginUser;

public sealed class LoginUserHandlerTests
{
    [Fact]
    public async Task HandleAsync_ShouldReturnSuccessfulResult()
    {
        var userId = Guid.NewGuid();

        var identityService =
            new Mock<IIdentityService>();

        identityService
            .Setup(service =>
                service.AuthenticateUserAsync(
                    "michal@example.com",
                    "Test123!",
                    It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                AuthenticateUserResult.Success(
                    userId,
                    Language.English,
                    SubscriptionPlan.Free));

        var accessTokenGenerator =
            new Mock<IAccessTokenGenerator>();

        accessTokenGenerator
            .Setup(generator =>
                generator.GenerateToken(userId))
            .Returns("access-token");

        var handler = new LoginUserHandler(
            identityService.Object,
            accessTokenGenerator.Object);

        var command = new LoginUserCommand(
            "michal@example.com",
            "Test123!");

        var result =
            await handler.HandleAsync(command);

        Assert.True(result.Succeeded);
        Assert.Equal(
            "access-token",
            result.AccessToken);
        Assert.Equal(
            Language.English,
            result.Language);
        Assert.Equal(
            SubscriptionPlan.Free,
            result.SubscriptionPlan);
        Assert.Empty(result.Errors);

        identityService.Verify(
            service =>
                service.AuthenticateUserAsync(
                    "michal@example.com",
                    "Test123!",
                    It.IsAny<CancellationToken>()),
            Times.Once);

        accessTokenGenerator.Verify(
            generator =>
                generator.GenerateToken(userId),
            Times.Once);
    }

    [Fact]
    public async Task HandleAsync_ShouldReturnFailureResult()
    {
        var errors = new[]
        {
            new IdentityServiceError(
                "InvalidCredentials",
                "The email address or password is invalid.")
        };

        var identityService =
            new Mock<IIdentityService>();

        identityService
            .Setup(service =>
                service.AuthenticateUserAsync(
                    "michal@example.com",
                    "WrongPassword!",
                    It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                AuthenticateUserResult.Failure(errors));

        var accessTokenGenerator =
            new Mock<IAccessTokenGenerator>();

        var handler = new LoginUserHandler(
            identityService.Object,
            accessTokenGenerator.Object);

        var command = new LoginUserCommand(
            "michal@example.com",
            "WrongPassword!");

        var result =
            await handler.HandleAsync(command);

        Assert.False(result.Succeeded);
        Assert.Null(result.AccessToken);
        Assert.Null(result.Language);
        Assert.Null(result.SubscriptionPlan);

        var error = Assert.Single(result.Errors);

        Assert.Equal(
            "InvalidCredentials",
            error.Code);

        Assert.Equal(
            "The email address or password is invalid.",
            error.Description);

        identityService.Verify(
            service =>
                service.AuthenticateUserAsync(
                    "michal@example.com",
                    "WrongPassword!",
                    It.IsAny<CancellationToken>()),
            Times.Once);

        accessTokenGenerator.Verify(
            generator =>
                generator.GenerateToken(
                    It.IsAny<Guid>()),
            Times.Never);
    }
}
