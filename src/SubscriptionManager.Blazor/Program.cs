using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.Options;
using SubscriptionManager.Blazor.Components;
using SubscriptionManager.Blazor.Configuration;
using SubscriptionManager.Blazor.Features.Authentication;
using SubscriptionManager.Blazor.Features.DigitalServices;
using SubscriptionManager.Blazor.Features.Subscriptions;
using SubscriptionManager.Blazor.Services;

var builder = WebApplication.CreateBuilder(args);

var authenticationOptions =
    builder.Configuration
        .GetSection(AuthenticationCookieOptions.SectionName)
        .Get<AuthenticationCookieOptions>()
    ?? throw new InvalidOperationException(
        "Authentication configuration is missing.");

builder.Services
    .AddRazorComponents()
    .AddInteractiveServerComponents(options =>
    {
        options.DetailedErrors =
            builder.Environment.IsDevelopment();
    });

builder.Services.Configure<ApiOptions>(
    builder.Configuration.GetSection(
        ApiOptions.SectionName));

builder.Services.Configure<AuthenticationCookieOptions>(
    builder.Configuration.GetSection(
        AuthenticationCookieOptions.SectionName));

builder.Services.Configure<RequestLocalizationOptions>(
    options =>
    {
        options.DefaultRequestCulture =
            new RequestCulture(
                SupportedCultures.DefaultCulture);

        options.SupportedCultures =
            SupportedCultures.All.ToList();

        options.SupportedUICultures =
            SupportedCultures.All.ToList();

        options.RequestCultureProviders =
        [
            new CookieRequestCultureProvider()
        ];
    });

builder.Services
    .AddAuthentication(
        CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.Name =
            "__Host-SubscriptionManager.Authentication.v3";

        options.Cookie.HttpOnly = true;
        options.Cookie.IsEssential = true;
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.Cookie.SecurePolicy =
            CookieSecurePolicy.Always;

        options.LoginPath = "/login";
        options.AccessDeniedPath = "/login";
        options.ReturnUrlParameter = "returnUrl";

        options.ExpireTimeSpan =
            TimeSpan.FromMinutes(
                authenticationOptions
                    .AuthenticationCookieExpirationInMinutes);

        options.SlidingExpiration = false;

        options.Events.OnValidatePrincipal = context =>
        {
            var accessToken = context.Principal?
                .FindFirst(
                    AuthenticationClaimTypes.AccessToken)?
                .Value;

            if (string.IsNullOrWhiteSpace(accessToken))
            {
                context.RejectPrincipal();
            }

            return Task.CompletedTask;
        };
    });

builder.Services.AddAuthorization();
builder.Services.AddCascadingAuthenticationState();

builder.Services.AddHttpContextAccessor();

builder.Services.AddScoped<AppState>();
builder.Services.AddScoped<Localizer>();

builder.Services.AddHttpClient<SubscriptionsApiClient>(
    (serviceProvider, client) =>
    {
        var options = serviceProvider
            .GetRequiredService<IOptions<ApiOptions>>()
            .Value;

        client.BaseAddress =
            new Uri(options.BaseUrl);
    });

builder.Services.AddHttpClient<DigitalServicesApiClient>(
    (serviceProvider, client) =>
    {
        var options = serviceProvider
            .GetRequiredService<IOptions<ApiOptions>>()
            .Value;

        client.BaseAddress =
            new Uri(options.BaseUrl);
    });

builder.Services.AddHttpClient<AuthenticationApiClient>(
    (serviceProvider, client) =>
    {
        var options = serviceProvider
            .GetRequiredService<IOptions<ApiOptions>>()
            .Value;

        client.BaseAddress =
            new Uri(options.BaseUrl);
    });

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler(
        "/Error",
        createScopeForErrors: true);

    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute(
    "/not-found",
    createScopeForStatusCodePages: true);

app.UseHttpsRedirection();
app.UseRequestLocalization();

app.UseAuthentication();
app.UseAuthorization();

app.UseAntiforgery();

app.MapGet(
    "/culture/set",
    (
        string culture,
        string? redirectUri,
        HttpContext context) =>
    {
        var selectedCulture =
            SupportedCultures.Contains(culture)
                ? culture
                : SupportedCultures.DefaultCulture;

        context.Response.Cookies.Append(
            CookieRequestCultureProvider
                .DefaultCookieName,
            CookieRequestCultureProvider
                .MakeCookieValue(
                    new RequestCulture(
                        selectedCulture)),
            new CookieOptions
            {
                Expires =
                    DateTimeOffset.UtcNow.AddYears(1),

                IsEssential = true,
                SameSite = SameSiteMode.Lax,
                Secure = context.Request.IsHttps
            });

        var safeRedirect =
            !string.IsNullOrWhiteSpace(
                redirectUri) &&
            redirectUri.StartsWith('/') &&
            !redirectUri.StartsWith("//")
                ? redirectUri
                : "/";

        return Results.LocalRedirect(
            safeRedirect);
    });

app.MapStaticAssets();

app.MapAuthenticationEndpoints();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();

public partial class Program;
