using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using SubscriptionManager.Infrastructure.Identity;
using System.Net;
using System.Net.Http.Json;

namespace SubscriptionManager.Api.Tests.Authentication;

public sealed class PostLoginEndpointTests
    : IClassFixture<CustomWebApplicationFactory>
{
    private const string Password = "Test123!";

    private readonly CustomWebApplicationFactory _factory;

    public PostLoginEndpointTests(
        CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task PostAsync_ShouldReturnAccessToken_WhenCredentialsAreValid()
    {
        var email =
            $"confirmed-{Guid.NewGuid()}@example.com";

        await CreateUserAsync(
            email,
            Password,
            emailConfirmed: true);

        using var client =
            _factory.CreateUnauthenticatedClient();

        var request = new LoginRequest(
            email,
            Password);

        var response = await client.PostAsJsonAsync(
            "/api/auth/login",
            request);

        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode);

        var loginResponse = await response.Content
            .ReadFromJsonAsync<LoginResponse>();

        Assert.NotNull(loginResponse);

        Assert.False(
            string.IsNullOrWhiteSpace(
                loginResponse.AccessToken));

        Assert.Equal(
            "Free",
            loginResponse.SubscriptionPlan);
    }

    [Fact]
    public async Task PostAsync_ShouldReturnBadRequest_WhenCredentialsAreInvalid()
    {
        var email =
            $"confirmed-{Guid.NewGuid()}@example.com";

        await CreateUserAsync(
            email,
            Password,
            emailConfirmed: true);

        using var client =
            _factory.CreateUnauthenticatedClient();

        var request = new LoginRequest(
            email,
            "WrongPassword123!");

        var response = await client.PostAsJsonAsync(
            "/api/auth/login",
            request);

        Assert.Equal(
            HttpStatusCode.BadRequest,
            response.StatusCode);

        var problemDetails = await response.Content
            .ReadFromJsonAsync<HttpValidationProblemDetails>();

        Assert.NotNull(problemDetails);

        Assert.Equal(
            "The email address or password is invalid.",
            Assert.Single(
                problemDetails.Errors[
                    "InvalidCredentials"]));
    }

    [Fact]
    public async Task PostAsync_ShouldReturnBadRequest_WhenEmailIsNotConfirmed()
    {
        var email =
            $"unconfirmed-{Guid.NewGuid()}@example.com";

        await CreateUserAsync(
            email,
            Password,
            emailConfirmed: false);

        using var client =
            _factory.CreateUnauthenticatedClient();

        var request = new LoginRequest(
            email,
            Password);

        var response = await client.PostAsJsonAsync(
            "/api/auth/login",
            request);

        Assert.Equal(
            HttpStatusCode.BadRequest,
            response.StatusCode);

        var problemDetails = await response.Content
            .ReadFromJsonAsync<HttpValidationProblemDetails>();

        Assert.NotNull(problemDetails);

        Assert.Equal(
            "The email address has not been confirmed.",
            Assert.Single(
                problemDetails.Errors[
                    "EmailNotConfirmed"]));
    }

    private async Task CreateUserAsync(
        string email,
        string password,
        bool emailConfirmed)
    {
        using var scope =
            _factory.Services.CreateScope();

        var userManager = scope.ServiceProvider
            .GetRequiredService<
                UserManager<ApplicationUser>>();

        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = email,
            Email = email,
            EmailConfirmed = emailConfirmed
        };

        var result = await userManager.CreateAsync(
            user,
            password);

        Assert.True(
            result.Succeeded,
            string.Join(
                Environment.NewLine,
                result.Errors.Select(
                    error =>
                        $"{error.Code}: {error.Description}")));
    }

    private sealed record LoginRequest(
        string Email,
        string Password);

    private sealed record LoginResponse(
        string AccessToken,
        string SubscriptionPlan);
}
