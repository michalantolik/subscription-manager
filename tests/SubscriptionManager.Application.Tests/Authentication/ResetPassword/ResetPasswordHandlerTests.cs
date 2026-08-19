using Moq;
using SubscriptionManager.Application.Account;
using SubscriptionManager.Application.Authentication;
using SubscriptionManager.Application.Authentication.ResetPassword;
using SubscriptionManager.Application.Common.Identity;
using SubscriptionManager.Application.Common.Localization;
using SubscriptionManager.Domain.Subscriptions;

namespace SubscriptionManager.Application.Tests.Authentication.ResetPassword;

public sealed class ResetPasswordHandlerTests
{
    [Fact]
    public async Task HandleAsync_ShouldSendPasswordChangedEmail()
    {
        var userId = Guid.NewGuid();

        var identityService = new Mock<IIdentityService>();

        identityService
            .Setup(service => service.GetEmailAsync(
                userId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync("user@example.com");

        identityService
            .Setup(service => service.GetAccountPreferencesAsync(
                userId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                new AccountPreferences(
                    Language.English,
                    Currency.EUR));

        identityService
            .Setup(service => service.ResetPasswordAsync(
                userId,
                "reset-token",
                "NewPassword123!",
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(ResetPasswordResult.Success());

        var emailSender = new Mock<IEmailSender>();

        var handler = new ResetPasswordHandler(
            identityService.Object,
            emailSender.Object);

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

        emailSender.Verify(
            sender => sender.SendPasswordChangedAsync(
                "user@example.com",
                "en",
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleAsync_ShouldNotSendEmailWhenResetFails()
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
            .Setup(service => service.GetEmailAsync(
                userId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync("user@example.com");

        identityService
            .Setup(service => service.GetAccountPreferencesAsync(
                userId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                new AccountPreferences(
                    Language.English,
                    Currency.EUR));

        identityService
            .Setup(service => service.ResetPasswordAsync(
                userId,
                "invalid-token",
                "NewPassword123!",
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                ResetPasswordResult.Failure(errors));

        var emailSender = new Mock<IEmailSender>();

        var handler = new ResetPasswordHandler(
            identityService.Object,
            emailSender.Object);

        var command = new ResetPasswordCommand(
            userId,
            "invalid-token",
            "NewPassword123!");

        var result = await handler.HandleAsync(command);

        Assert.False(result.Succeeded);

        var error = Assert.Single(result.Errors);

        Assert.Equal("InvalidToken", error.Code);
        Assert.Equal("Invalid token.", error.Description);

        emailSender.Verify(
            sender => sender.SendPasswordChangedAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task HandleAsync_ShouldReturnSuccessWhenEmailSendingFails()
    {
        var userId = Guid.NewGuid();

        var identityService = new Mock<IIdentityService>();

        identityService
            .Setup(service => service.GetEmailAsync(
                userId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync("user@example.com");

        identityService
            .Setup(service => service.GetAccountPreferencesAsync(
                userId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                new AccountPreferences(
                    Language.Polish,
                    Currency.PLN));

        identityService
            .Setup(service => service.ResetPasswordAsync(
                userId,
                "reset-token",
                "NewPassword123!",
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(ResetPasswordResult.Success());

        var emailSender = new Mock<IEmailSender>();

        emailSender
            .Setup(sender => sender.SendPasswordChangedAsync(
                "user@example.com",
                "pl",
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(
                new InvalidOperationException(
                    "Email delivery failed."));

        var handler = new ResetPasswordHandler(
            identityService.Object,
            emailSender.Object);

        var command = new ResetPasswordCommand(
            userId,
            "reset-token",
            "NewPassword123!");

        var result = await handler.HandleAsync(command);

        Assert.True(result.Succeeded);
        Assert.Empty(result.Errors);

        emailSender.Verify(
            sender => sender.SendPasswordChangedAsync(
                "user@example.com",
                "pl",
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
