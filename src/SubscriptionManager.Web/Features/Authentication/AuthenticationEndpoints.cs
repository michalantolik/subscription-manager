using System.Security.Claims;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.Options;
using SubscriptionManager.Web.Common.Currencies;
using SubscriptionManager.Web.Common.Localization;
using SubscriptionManager.Web.Features.Authentication.Security;

namespace SubscriptionManager.Web.Features.Authentication;

/// <summary>
/// Provides authentication endpoints for the web application.
/// </summary>
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

        endpoints.MapGet(
            "/authentication/account-deleted",
            AccountDeletedAsync);

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

        var languageIsValid =
            Enum.TryParse<Language>(
                form["language"].ToString(),
                ignoreCase: true,
                out var language) &&
            Enum.IsDefined(language);

        var baseCurrencyIsValid =
            Enum.TryParse<Currency>(
                form["baseCurrency"].ToString(),
                ignoreCase: true,
                out var baseCurrency) &&
            Enum.IsDefined(baseCurrency);

        if (string.IsNullOrWhiteSpace(email) ||
            string.IsNullOrWhiteSpace(password) ||
            string.IsNullOrWhiteSpace(confirmPassword) ||
            !languageIsValid ||
            !baseCurrencyIsValid)
        {
            return RedirectToRegister(
            [
                AuthenticationErrorCodes.Required
            ]);
        }

        if (!string.Equals(
                password,
                confirmPassword,
                StringComparison.Ordinal))
        {
            return RedirectToRegister(
            [
                AuthenticationErrorCodes.PasswordMismatch
            ]);
        }

        AuthenticationOperationResult registerResult;

        try
        {
            registerResult =
                await authenticationApiClient.RegisterAsync(
                    email,
                    password,
                    language,
                    baseCurrency,
                    cancellationToken);
        }
        catch (HttpRequestException)
        {
            return RedirectToRegister(
            [
                AuthenticationErrorCodes.ServiceUnavailable
            ]);
        }

        if (!registerResult.Succeeded)
        {
            return RedirectToRegister(
                registerResult.Errors.Select(
                    error => error.Code));
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
            string.IsNullOrWhiteSpace(
                loginResult.AccessToken) ||
            loginResult.Language is null ||
            string.IsNullOrWhiteSpace(
                loginResult.SubscriptionPlan))
        {
            var errorCode =
                AuthenticationErrorCodes.Normalize(
                        loginResult.Errors.Select(
                            error => error.Code))
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
                loginResult.AccessToken),

            new Claim(
                AuthenticationClaimTypes.SubscriptionPlan,
                loginResult.SubscriptionPlan)
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

        SetCultureCookie(
            context,
            loginResult.Language.Value);

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

    private static async Task<IResult> AccountDeletedAsync(
        HttpContext context,
        CancellationToken cancellationToken)
    {
        await context.SignOutAsync(
            CookieAuthenticationDefaults.AuthenticationScheme);

        return Results.LocalRedirect(
            "/login?status=account-deleted");
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

    private static void SetCultureCookie(
        HttpContext context,
        Language language)
    {
        var cultureName =
            language.ToCultureName();

        context.Response.Cookies.Append(
            CookieRequestCultureProvider
                .DefaultCookieName,
            CookieRequestCultureProvider
                .MakeCookieValue(
                    new RequestCulture(
                        cultureName)),
            new CookieOptions
            {
                Expires =
                    DateTimeOffset.UtcNow.AddYears(1),

                IsEssential = true,
                SameSite = SameSiteMode.Lax,
                Secure = context.Request.IsHttps
            });
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
            return "/overview";
        }

        return returnUrl;
    }
}
