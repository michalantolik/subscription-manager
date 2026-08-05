using Moq;
using SubscriptionManager.Application.Common.Email;
using SubscriptionManager.Application.Common.Identity;
using SubscriptionManager.Application.Common.Localization;
using SubscriptionManager.Application.Identity.RegisterUser;
using SubscriptionManager.Domain.Subscriptions;

namespace SubscriptionManager.Application.Tests.Identity.RegisterUser;

public sealed class RegisterUserHandlerTests
{
    [Theory]
    [InlineData(Language.Polish, Currency.PLN, "pl")]
    [InlineData(Language.English, Currency.USD, "en")]
    [InlineData(Language.German, Currency.EUR, "de")]
    public async Task HandleAsync_ShouldCreateUserWithSelectedPreferences(
        Language language,
        Currency baseCurrency,
        string expectedLanguageCode)
    {
        var userId = Guid.NewGuid();

        var identityService =
            new Mock<IIdentityService>();

        identityService
            .Setup(service =>
                service.CreateUserAsync(
                    "michal@example.com",
                    "Test123!",
                    language,
                    baseCurrency,
                    It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                CreateUserResult.Success(userId));

        identityService
            .Setup(service =>
                service.GenerateEmailConfirmationTokenAsync(
                    userId,
                    It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                "confirmation-token");

        var emailSender =
            new Mock<IEmailSender>();

        var handler = new RegisterUserHandler(
            identityService.Object,
            emailSender.Object);

        var command = new RegisterUserCommand(
            "michal@example.com",
            "Test123!",
            language,
            baseCurrency);

        var result =
            await handler.HandleAsync(command);

        Assert.True(result.Succeeded);
        Assert.Equal(userId, result.UserId);
        Assert.Empty(result.Errors);

        identityService.Verify(
            service => service.CreateUserAsync(
                "michal@example.com",
                "Test123!",
                language,
                baseCurrency,
                It.IsAny<CancellationToken>()),
            Times.Once);

        identityService.Verify(
            service =>
                service.GenerateEmailConfirmationTokenAsync(
                    userId,
                    It.IsAny<CancellationToken>()),
            Times.Once);

        emailSender.Verify(
            sender =>
                sender.SendEmailConfirmationAsync(
                    "michal@example.com",
                    userId,
                    "confirmation-token",
                    expectedLanguageCode,
                    It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleAsync_ShouldReturnFailureResult()
    {
        var errors = new[]
        {
            new IdentityServiceError(
                "DuplicateEmail",
                "Email is already taken.")
        };

        var identityService =
            new Mock<IIdentityService>();

        identityService
            .Setup(service =>
                service.CreateUserAsync(
                    "michal@example.com",
                    "Test123!",
                    Language.Polish,
                    Currency.PLN,
                    It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                CreateUserResult.Failure(errors));

        var emailSender =
            new Mock<IEmailSender>();

        var handler = new RegisterUserHandler(
            identityService.Object,
            emailSender.Object);

        var command = new RegisterUserCommand(
            "michal@example.com",
            "Test123!",
            Language.Polish,
            Currency.PLN);

        var result =
            await handler.HandleAsync(command);

        Assert.False(result.Succeeded);
        Assert.Null(result.UserId);

        var error = Assert.Single(result.Errors);

        Assert.Equal(
            "DuplicateEmail",
            error.Code);

        Assert.Equal(
            "Email is already taken.",
            error.Description);

        identityService.Verify(
            service => service.CreateUserAsync(
                "michal@example.com",
                "Test123!",
                Language.Polish,
                Currency.PLN,
                It.IsAny<CancellationToken>()),
            Times.Once);

        identityService.Verify(
            service =>
                service.GenerateEmailConfirmationTokenAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<CancellationToken>()),
            Times.Never);

        emailSender.Verify(
            sender =>
                sender.SendEmailConfirmationAsync(
                    It.IsAny<string>(),
                    It.IsAny<Guid>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()),
            Times.Never);
    }
}
