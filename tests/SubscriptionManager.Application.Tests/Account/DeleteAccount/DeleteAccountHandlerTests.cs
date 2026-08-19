using Moq;
using SubscriptionManager.Application.Account;
using SubscriptionManager.Application.Account.DeleteAccount;
using SubscriptionManager.Application.Authentication;
using SubscriptionManager.Application.Common.Identity;
using SubscriptionManager.Application.Common.Localization;
using SubscriptionManager.Domain.Subscriptions;

namespace SubscriptionManager.Application.Tests.Account.DeleteAccount;

public sealed class DeleteAccountHandlerTests
{
    [Fact]
    public async Task HandleAsync_ShouldSendAccountDeletedEmail()
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
                    Language.German,
                    Currency.EUR));

        identityService
            .Setup(service => service.DeleteUserAsync(
                userId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(DeleteUserResult.Success());

        var emailSender = new Mock<IEmailSender>();

        var handler = new DeleteAccountHandler(
            identityService.Object,
            emailSender.Object);

        var command = new DeleteAccountCommand(userId);

        var result = await handler.HandleAsync(command);

        Assert.True(result.Succeeded);
        Assert.Empty(result.Errors);

        identityService.Verify(
            service => service.DeleteUserAsync(
                userId,
                It.IsAny<CancellationToken>()),
            Times.Once);

        emailSender.Verify(
            sender => sender.SendAccountDeletedAsync(
                "user@example.com",
                "de",
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleAsync_ShouldNotSendEmailWhenDeletionFails()
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
                    Currency.GBP));

        identityService
            .Setup(service => service.DeleteUserAsync(
                userId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                DeleteUserResult.Failure(errors));

        var emailSender = new Mock<IEmailSender>();

        var handler = new DeleteAccountHandler(
            identityService.Object,
            emailSender.Object);

        var command = new DeleteAccountCommand(userId);

        var result = await handler.HandleAsync(command);

        Assert.False(result.Succeeded);

        var error = Assert.Single(result.Errors);

        Assert.Equal("UserNotFound", error.Code);
        Assert.Equal("The user was not found.", error.Description);

        emailSender.Verify(
            sender => sender.SendAccountDeletedAsync(
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
            .Setup(service => service.DeleteUserAsync(
                userId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(DeleteUserResult.Success());

        var emailSender = new Mock<IEmailSender>();

        emailSender
            .Setup(sender => sender.SendAccountDeletedAsync(
                "user@example.com",
                "pl",
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(
                new InvalidOperationException(
                    "Email delivery failed."));

        var handler = new DeleteAccountHandler(
            identityService.Object,
            emailSender.Object);

        var command = new DeleteAccountCommand(userId);

        var result = await handler.HandleAsync(command);

        Assert.True(result.Succeeded);
        Assert.Empty(result.Errors);

        emailSender.Verify(
            sender => sender.SendAccountDeletedAsync(
                "user@example.com",
                "pl",
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
