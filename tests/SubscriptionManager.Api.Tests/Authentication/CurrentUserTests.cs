using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using SubscriptionManager.Api.Authentication;

namespace SubscriptionManager.Api.Tests.Authentication;

public sealed class CurrentUserTests
{
    [Fact]
    public void UserId_ShouldReturnIdentifierFromSubClaim()
    {
        var expectedUserId = Guid.NewGuid();

        var httpContext = new DefaultHttpContext
        {
            User = new ClaimsPrincipal(
                new ClaimsIdentity(
                [
                    new Claim(
                        JwtRegisteredClaimNames.Sub,
                        expectedUserId.ToString())
                ]))
        };

        var httpContextAccessor = new HttpContextAccessor
        {
            HttpContext = httpContext
        };

        var currentUser = new CurrentUser(
            httpContextAccessor);

        var userId = currentUser.UserId;

        Assert.Equal(expectedUserId, userId);
    }

    [Fact]
    public void UserId_ShouldThrow_WhenSubClaimIsMissing()
    {
        var httpContext = new DefaultHttpContext
        {
            User = new ClaimsPrincipal(
                new ClaimsIdentity())
        };

        var httpContextAccessor = new HttpContextAccessor
        {
            HttpContext = httpContext
        };

        var currentUser = new CurrentUser(
            httpContextAccessor);

        var exception = Assert.Throws<InvalidOperationException>(
            () => currentUser.UserId);

        Assert.Equal(
            "The current user identifier is unavailable.",
            exception.Message);
    }

    [Fact]
    public void UserId_ShouldThrow_WhenSubClaimIsNotGuid()
    {
        var httpContext = new DefaultHttpContext
        {
            User = new ClaimsPrincipal(
                new ClaimsIdentity(
                [
                    new Claim(
                        JwtRegisteredClaimNames.Sub,
                        "invalid-user-id")
                ]))
        };

        var httpContextAccessor = new HttpContextAccessor
        {
            HttpContext = httpContext
        };

        var currentUser = new CurrentUser(
            httpContextAccessor);

        var exception = Assert.Throws<InvalidOperationException>(
            () => currentUser.UserId);

        Assert.Equal(
            "The current user identifier is unavailable.",
            exception.Message);
    }
}
