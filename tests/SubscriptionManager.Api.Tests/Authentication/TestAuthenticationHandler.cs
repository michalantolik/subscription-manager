using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace SubscriptionManager.Api.Tests.Authentication;

public sealed class TestAuthenticationHandler
    : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public const string AuthenticationScheme = "Test";

    public const string UserIdHeaderName =
        "X-Test-User-Id";

    public static readonly Guid DefaultUserId =
        Guid.Parse(
            "11111111-1111-1111-1111-111111111111");

    public TestAuthenticationHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder)
        : base(options, logger, encoder)
    {
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var userId = DefaultUserId;

        if (Request.Headers.TryGetValue(
                UserIdHeaderName,
                out var userIdHeader))
        {
            if (!Guid.TryParse(
                    userIdHeader.ToString(),
                    out userId))
            {
                return Task.FromResult(
                    AuthenticateResult.Fail(
                        "The test user identifier is invalid."));
            }
        }

        var claims = new[]
        {
            new Claim(
                JwtRegisteredClaimNames.Sub,
                userId.ToString())
        };

        var identity = new ClaimsIdentity(
            claims,
            AuthenticationScheme);

        var principal = new ClaimsPrincipal(identity);

        var ticket = new AuthenticationTicket(
            principal,
            AuthenticationScheme);

        return Task.FromResult(
            AuthenticateResult.Success(ticket));
    }
}
