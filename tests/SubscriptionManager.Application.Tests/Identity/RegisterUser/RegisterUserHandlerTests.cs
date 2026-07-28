using SubscriptionManager.Application.Common.Email;
using SubscriptionManager.Application.Common.Identity;
using SubscriptionManager.Application.Identity.RegisterUser;

namespace SubscriptionManager.Application.Tests.Identity.RegisterUser;

public sealed class RegisterUserHandlerTests
{
    [Fact]
    public async Task HandleAsync_ShouldReturnSuccessfulResult()
    {
        var userId = Guid.NewGuid();

        var identityService = new TestIdentityService(
            CreateUserResult.Success(userId),
            "confirmation-token");

        var emailSender = new TestEmailSender();

        var handler = new RegisterUserHandler(
            identityService,
            emailSender);

        var command = new RegisterUserCommand(
            "michal@example.com",
            "Test123!");

        var result = await handler.HandleAsync(command);

        Assert.True(result.Succeeded);
        Assert.Equal(userId, result.UserId);
        Assert.Empty(result.Errors);

        Assert.True(emailSender.WasCalled);
        Assert.Equal("michal@example.com", emailSender.Email);
        Assert.Equal(userId, emailSender.UserId);
        Assert.Equal("confirmation-token", emailSender.ConfirmationToken);
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

        var identityService = new TestIdentityService(
            CreateUserResult.Failure(errors));

        var emailSender = new TestEmailSender();

        var handler = new RegisterUserHandler(
            identityService,
            emailSender);

        var command = new RegisterUserCommand(
            "michal@example.com",
            "Test123!");

        var result = await handler.HandleAsync(command);

        Assert.False(result.Succeeded);
        Assert.Null(result.UserId);

        var error = Assert.Single(result.Errors);

        Assert.Equal("DuplicateEmail", error.Code);
        Assert.Equal("Email is already taken.", error.Description);

        Assert.False(emailSender.WasCalled);
    }

    private sealed class TestIdentityService(
        CreateUserResult createUserResult,
        string? confirmationToken = null)
        : IIdentityService
    {
        public Task<CreateUserResult> CreateUserAsync(
            string email,
            string password,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(createUserResult);
        }

        public Task<string?> GenerateEmailConfirmationTokenAsync(
            Guid userId,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(confirmationToken);
        }

        public Task<ConfirmEmailResult> ConfirmEmailAsync(
            Guid userId,
            string confirmationToken,
            CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }

        public Task<AuthenticateUserResult> AuthenticateUserAsync(
            string email,
            string password,
            CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }
    }

    private sealed class TestEmailSender : IEmailSender
    {
        public bool WasCalled { get; private set; }

        public string? Email { get; private set; }

        public Guid? UserId { get; private set; }

        public string? ConfirmationToken { get; private set; }

        public Task SendEmailConfirmationAsync(
            string email,
            Guid userId,
            string confirmationToken,
            CancellationToken cancellationToken = default)
        {
            WasCalled = true;
            Email = email;
            UserId = userId;
            ConfirmationToken = confirmationToken;

            return Task.CompletedTask;
        }
    }
}
