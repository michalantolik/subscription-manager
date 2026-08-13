using Moq;
using SubscriptionManager.Application.Common.Identity;
using SubscriptionManager.Application.Authentication.ResetPassword;

namespace SubscriptionManager.Application.Tests.Authentication.ResetPassword;

public sealed class ResetPasswordHandlerTests
{
    [Fact]
    public async Task HandleAsync_ShouldReturnSuccessfulResult()
    {
        var userId = Guid.NewGuid();

        var identityService = new Mock<IIdentityService>();

        identityService
            .Setup(service => service.ResetPasswordAsync(
                userId,
                "reset-token",
                "NewPassword123!",
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(ResetPasswordResult.Success());

        var handler = new ResetPasswordHandler(
            identityService.Object);

        var command = new ResetPasswordCommand(
            userId,
            "reset-token",
            "NewPassword123!");

        var result = await handler.HandleAsync(command);

        Assert.True(result.Succeeded);
        Assert.Empty(result.Errors);

        identityService.Verify(
            service => service.ResetPasswordAsync(
                userId,
                "reset-token",
                "NewPassword123!",
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleAsync_ShouldReturnFailureResult()
    {
        var userId = Guid.NewGuid();

        var errors = new[]
        {
            new IdentityServiceError(
                "InvalidToken",
                "Invalid token.")
        };

        var identityService = new Mock<IIdentityService>();

        identityService
            .Setup(service => service.ResetPasswordAsync(
                userId,
                "invalid-token",
                "NewPassword123!",
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(ResetPasswordResult.Failure(errors));

        var handler = new ResetPasswordHandler(
            identityService.Object);

        var command = new ResetPasswordCommand(
            userId,
            "invalid-token",
            "NewPassword123!");

        var result = await handler.HandleAsync(command);

        Assert.False(result.Succeeded);

        var error = Assert.Single(result.Errors);

        Assert.Equal("InvalidToken", error.Code);
        Assert.Equal("Invalid token.", error.Description);

        identityService.Verify(
            service => service.ResetPasswordAsync(
                userId,
                "invalid-token",
                "NewPassword123!",
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
