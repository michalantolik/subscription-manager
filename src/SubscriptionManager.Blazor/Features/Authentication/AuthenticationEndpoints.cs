using System.Security.Claims;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.Options;
using SubscriptionManager.Blazor.Configuration;

namespace SubscriptionManager.Blazor.Features.Authentication;

public static class AuthenticationEndpoints
{
    public static IEndpointRouteBuilder MapAuthenticationEndpoints(
        this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost(
            "/authentication/register",
            RegisterAsync);

        endpoints.MapPost(
            "/authentication/login",
            LoginAsync);

        endpoints.MapPost(
            "/authentication/logout",
            LogoutAsync);

        endpoints.MapGet(
            "/authentication/session-expired",
            SessionExpiredAsync);

        return endpoints;
    }

    private static async Task<IResult> RegisterAsync(
        HttpContext context,
        AuthenticationApiClient authenticationApiClient,
        IAntiforgery antiforgery,
        CancellationToken cancellationToken)
    {
        await antiforgery.ValidateRequestAsync(context);

        var form = await context.Request.ReadFormAsync(
            cancellationToken);

        var email = form["email"]
            .ToString()
            .Trim();

        var password = form["password"]
            .ToString();

        var confirmPassword = form["confirmPassword"]
            .ToString();

        var languageCode = NormalizeLanguageCode(
            form["languageCode"].ToString());

        if (string.IsNullOrWhiteSpace(email) ||
            string.IsNullOrWhiteSpace(password) ||
            string.IsNullOrWhiteSpace(confirmPassword))
        {
            return RedirectToRegister([AuthenticationErrorCodes.Required]);
        }

        if (!string.Equals(
                password,
                confirmPassword,
                StringComparison.Ordinal))
        {
            return RedirectToRegister([AuthenticationErrorCodes.PasswordMismatch]);
        }

        AuthenticationOperationResult registerResult;

        try
        {
            registerResult =
                await authenticationApiClient.RegisterAsync(
                    email,
                    password,
                    languageCode,
                    cancellationToken);
        }
        catch (HttpRequestException)
        {
            return RedirectToRegister([AuthenticationErrorCodes.ServiceUnavailable]);
        }

        if (!registerResult.Succeeded)
        {
            return RedirectToRegister(
                registerResult.Errors.Select(error => error.Code));
        }

        return Results.LocalRedirect(
            "/register?status=created");
    }

    private static async Task<IResult> LoginAsync(
        HttpContext context,
        AuthenticationApiClient authenticationApiClient,
        IOptions<AuthenticationCookieOptions> authenticationOptions,
        IAntiforgery antiforgery,
        CancellationToken cancellationToken)
    {
        await antiforgery.ValidateRequestAsync(context);

        var form = await context.Request.ReadFormAsync(
            cancellationToken);

        var email = form["email"]
            .ToString()
            .Trim();

        var password = form["password"]
            .ToString();

        var returnUrl = GetSafeReturnUrl(
            form["returnUrl"].ToString());

        if (string.IsNullOrWhiteSpace(email) ||
            string.IsNullOrWhiteSpace(password))
        {
            return RedirectToLogin(
                returnUrl,
                AuthenticationErrorCodes.Required);
        }

        LoginOperationResult loginResult;

        try
        {
            loginResult =
                await authenticationApiClient.LoginAsync(
                    email,
                    password,
                    cancellationToken);
        }
        catch (HttpRequestException)
        {
            return RedirectToLogin(
                returnUrl,
                AuthenticationErrorCodes.ServiceUnavailable);
        }

        if (!loginResult.Succeeded ||
            string.IsNullOrWhiteSpace(loginResult.AccessToken))
        {
            var errorCode = AuthenticationErrorCodes.Normalize(
                    loginResult.Errors.Select(error => error.Code))
                .First();

            return RedirectToLogin(
                returnUrl,
                errorCode);
        }

        var claims = new[]
        {
            new Claim(
                ClaimTypes.Name,
                email),

            new Claim(
                ClaimTypes.Email,
                email),

            new Claim(
                AuthenticationClaimTypes.AccessToken,
                loginResult.AccessToken)
        };

        var identity = new ClaimsIdentity(
            claims,
            CookieAuthenticationDefaults.AuthenticationScheme);

        var principal = new ClaimsPrincipal(identity);

        var authenticationProperties =
            new AuthenticationProperties
            {
                AllowRefresh = false,
                IsPersistent = false,
                IssuedUtc = DateTimeOffset.UtcNow,
                ExpiresUtc =
                    DateTimeOffset.UtcNow.AddMinutes(
                        authenticationOptions.Value
                            .AuthenticationCookieExpirationInMinutes)
            };

        await context.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            principal,
            authenticationProperties);

        return Results.LocalRedirect(returnUrl);
    }


    private static async Task<IResult> SessionExpiredAsync(
        HttpContext context,
        string? returnUrl)
    {
        await context.SignOutAsync(
            CookieAuthenticationDefaults.AuthenticationScheme);

        var safeReturnUrl = GetSafeReturnUrl(returnUrl);

        return RedirectToLogin(
            safeReturnUrl,
            AuthenticationErrorCodes.SessionExpired);
    }

    private static async Task<IResult> LogoutAsync(
        HttpContext context,
        IAntiforgery antiforgery)
    {
        await antiforgery.ValidateRequestAsync(context);

        await context.SignOutAsync(
            CookieAuthenticationDefaults.AuthenticationScheme);

        return Results.LocalRedirect("/login");
    }

    private static IResult RedirectToRegister(
        IEnumerable<string> errors)
    {
        var errorCodes = string.Join(
            ",",
            AuthenticationErrorCodes.Normalize(errors));

        var location =
            "/register" +
            $"?errors={Uri.EscapeDataString(errorCodes)}";

        return Results.LocalRedirect(location);
    }

    private static IResult RedirectToLogin(
        string returnUrl,
        string error)
    {
        var location =
            "/login" +
            $"?error={Uri.EscapeDataString(error)}" +
            $"&returnUrl={Uri.EscapeDataString(returnUrl)}";

        return Results.LocalRedirect(location);
    }

    private static string GetSafeReturnUrl(
        string? returnUrl)
    {
        if (string.IsNullOrWhiteSpace(returnUrl) ||
            !returnUrl.StartsWith(
                "/",
                StringComparison.Ordinal) ||
            returnUrl.StartsWith(
                "//",
                StringComparison.Ordinal))
        {
            return "/";
        }

        return returnUrl;
    }

    private static string NormalizeLanguageCode(
        string? languageCode)
    {
        return languageCode?.Trim().ToLowerInvariant() switch
        {
            "en" or "en-us" => "en",
            "de" or "de-de" => "de",
            _ => "pl"
        };
    }
}
