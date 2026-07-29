using Moq;
using SubscriptionManager.Application.Common.Email;
using SubscriptionManager.Application.Common.Identity;
using SubscriptionManager.Application.Identity.ForgotPassword;

namespace SubscriptionManager.Application.Tests.Identity.ForgotPassword;

public sealed class ForgotPasswordHandlerTests
{
    [Fact]
    public async Task HandleAsync_ShouldSendPasswordResetEmail()
    {
        var userId = Guid.NewGuid();

        var identityService = new Mock<IIdentityService>();

        identityService
            .Setup(service => service.GeneratePasswordResetTokenAsync(
                "michal@example.com",
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                new PasswordResetToken(
                    userId,
                    "michal@example.com",
                    "reset-token"));

        var emailSender = new Mock<IEmailSender>();

        var handler = new ForgotPasswordHandler(
            identityService.Object,
            emailSender.Object);

        var command = new ForgotPasswordCommand(
            "michal@example.com",
            "de");

        await handler.HandleAsync(command);

        identityService.Verify(
            service => service.GeneratePasswordResetTokenAsync(
                "michal@example.com",
                It.IsAny<CancellationToken>()),
            Times.Once);

        emailSender.Verify(
            sender => sender.SendPasswordResetAsync(
                "michal@example.com",
                userId,
                "reset-token",
                "de",
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleAsync_ShouldNotSendEmail_WhenUserDoesNotExist()
    {
        var identityService = new Mock<IIdentityService>();

        identityService
            .Setup(service => service.GeneratePasswordResetTokenAsync(
                "unknown@example.com",
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((PasswordResetToken?)null);

        var emailSender = new Mock<IEmailSender>();

        var handler = new ForgotPasswordHandler(
            identityService.Object,
            emailSender.Object);

        var command = new ForgotPasswordCommand(
            "unknown@example.com",
            "en");

        await handler.HandleAsync(command);

        identityService.Verify(
            service => service.GeneratePasswordResetTokenAsync(
                "unknown@example.com",
                It.IsAny<CancellationToken>()),
            Times.Once);

        emailSender.Verify(
            sender => sender.SendPasswordResetAsync(
                It.IsAny<string>(),
                It.IsAny<Guid>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }
}
