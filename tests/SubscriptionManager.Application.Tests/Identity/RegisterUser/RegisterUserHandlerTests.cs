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
            CreateUserResult.Success(userId));

        var handler = new RegisterUserHandler(identityService);

        var command = new RegisterUserCommand(
            "michal@example.com",
            "Test123!");

        var result = await handler.HandleAsync(command);

        Assert.True(result.Succeeded);
        Assert.Equal(userId, result.UserId);
        Assert.Empty(result.Errors);
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

        var handler = new RegisterUserHandler(identityService);

        var command = new RegisterUserCommand(
            "michal@example.com",
            "Test123!");

        var result = await handler.HandleAsync(command);

        Assert.False(result.Succeeded);
        Assert.Null(result.UserId);

        var error = Assert.Single(result.Errors);

        Assert.Equal("DuplicateEmail", error.Code);
        Assert.Equal("Email is already taken.", error.Description);
    }

    private sealed class TestIdentityService(
        CreateUserResult result)
        : IIdentityService
    {
        public Task<CreateUserResult> CreateUserAsync(
            string email,
            string password,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(result);
        }
    }
}
