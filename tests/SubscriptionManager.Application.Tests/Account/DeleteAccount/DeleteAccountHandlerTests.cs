using Moq;
using SubscriptionManager.Application.Common.Identity;
using SubscriptionManager.Application.Account.DeleteAccount;

namespace SubscriptionManager.Application.Tests.Account.DeleteAccount;

public sealed class DeleteAccountHandlerTests
{
    [Fact]
    public async Task HandleAsync_ShouldReturnSuccessfulResult()
    {
        var userId = Guid.NewGuid();

        var identityService = new Mock<IIdentityService>();

        identityService
            .Setup(service => service.DeleteUserAsync(
                userId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(DeleteUserResult.Success());

        var handler = new DeleteAccountHandler(
            identityService.Object);

        var command = new DeleteAccountCommand(userId);

        var result = await handler.HandleAsync(command);

        Assert.True(result.Succeeded);
        Assert.Empty(result.Errors);

        identityService.Verify(
            service => service.DeleteUserAsync(
                userId,
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
                "UserNotFound",
                "The user was not found.")
        };

        var identityService = new Mock<IIdentityService>();

        identityService
            .Setup(service => service.DeleteUserAsync(
                userId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(DeleteUserResult.Failure(errors));

        var handler = new DeleteAccountHandler(
            identityService.Object);

        var command = new DeleteAccountCommand(userId);

        var result = await handler.HandleAsync(command);

        Assert.False(result.Succeeded);

        var error = Assert.Single(result.Errors);

        Assert.Equal("UserNotFound", error.Code);
        Assert.Equal("The user was not found.", error.Description);

        identityService.Verify(
            service => service.DeleteUserAsync(
                userId,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
