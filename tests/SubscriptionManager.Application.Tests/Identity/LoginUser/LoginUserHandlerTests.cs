using SubscriptionManager.Application.Common.Authentication;
using SubscriptionManager.Application.Common.Identity;
using SubscriptionManager.Application.Identity.LoginUser;

namespace SubscriptionManager.Application.Tests.Identity.LoginUser;

public sealed class LoginUserHandlerTests
{
    [Fact]
    public async Task HandleAsync_ShouldReturnSuccessfulResult()
    {
        var userId = Guid.NewGuid();

        var identityService = new TestIdentityService(
            AuthenticateUserResult.Success(userId));

        var accessTokenGenerator = new TestAccessTokenGenerator(
            "access-token");

        var handler = new LoginUserHandler(
            identityService,
            accessTokenGenerator);

        var command = new LoginUserCommand(
            "michal@example.com",
            "Test123!");

        var result = await handler.HandleAsync(command);

        Assert.True(result.Succeeded);
        Assert.Equal("access-token", result.AccessToken);
        Assert.Empty(result.Errors);

        Assert.True(identityService.WasCalled);
        Assert.Equal("michal@example.com", identityService.Email);
        Assert.Equal("Test123!", identityService.Password);

        Assert.True(accessTokenGenerator.WasCalled);
        Assert.Equal(userId, accessTokenGenerator.UserId);
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

        var identityService = new TestIdentityService(
            AuthenticateUserResult.Failure(errors));

        var accessTokenGenerator = new TestAccessTokenGenerator(
            "access-token");

        var handler = new LoginUserHandler(
            identityService,
            accessTokenGenerator);

        var command = new LoginUserCommand(
            "michal@example.com",
            "WrongPassword!");

        var result = await handler.HandleAsync(command);

        Assert.False(result.Succeeded);
        Assert.Null(result.AccessToken);

        var error = Assert.Single(result.Errors);

        Assert.Equal("InvalidCredentials", error.Code);
        Assert.Equal(
            "The email address or password is invalid.",
            error.Description);

        Assert.True(identityService.WasCalled);
        Assert.Equal("michal@example.com", identityService.Email);
        Assert.Equal("WrongPassword!", identityService.Password);

        Assert.False(accessTokenGenerator.WasCalled);
    }

    private sealed class TestIdentityService(
        AuthenticateUserResult authenticateUserResult)
        : IIdentityService
    {
        public bool WasCalled { get; private set; }

        public string? Email { get; private set; }

        public string? Password { get; private set; }

        public Task<CreateUserResult> CreateUserAsync(
            string email,
            string password,
            CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }

        public Task<string?> GenerateEmailConfirmationTokenAsync(
            Guid userId,
            CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
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
            WasCalled = true;
            Email = email;
            Password = password;

            return Task.FromResult(authenticateUserResult);
        }
    }

    private sealed class TestAccessTokenGenerator(
        string accessToken)
        : IAccessTokenGenerator
    {
        public bool WasCalled { get; private set; }

        public Guid? UserId { get; private set; }

        public string GenerateToken(Guid userId)
        {
            WasCalled = true;
            UserId = userId;

            return accessToken;
        }
    }
}
