using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.Options;
using SubscriptionManager.Blazor.Components;
using SubscriptionManager.Blazor.Configuration;
using SubscriptionManager.Blazor.Features.Account;
using SubscriptionManager.Blazor.Features.Authentication;
using SubscriptionManager.Blazor.Features.Billing;
using SubscriptionManager.Blazor.Features.DigitalServices;
using SubscriptionManager.Blazor.Features.SavingsPlans;
using SubscriptionManager.Blazor.Features.Subscriptions;
using SubscriptionManager.Blazor.Services;

var builder = WebApplication.CreateBuilder(args);

var applicationInsightsConnectionString =
    builder.Configuration[
        "ApplicationInsights:ConnectionString"];

if (!string.IsNullOrWhiteSpace(
        applicationInsightsConnectionString))
{
    builder.Services.AddApplicationInsightsTelemetry();
}

var apiRequestTimeout =
    TimeSpan.FromSeconds(45);

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

builder.Services
    .AddOptions<ApiOptions>()
    .Bind(
        builder.Configuration.GetSection(
            ApiOptions.SectionName))
    .Validate(
        options =>
            Uri.TryCreate(
                options.BaseUrl,
                UriKind.Absolute,
                out var baseUri) &&
            baseUri.Scheme is "http" or "https",
        "Api:BaseUrl must be an absolute HTTP or HTTPS URL.")
    .ValidateOnStart();

builder.Services
    .AddOptions<AuthenticationCookieOptions>()
    .Bind(
        builder.Configuration.GetSection(
            AuthenticationCookieOptions.SectionName))
    .Validate(
        options =>
            options.AuthenticationCookieExpirationInMinutes > 0,
        "Authentication:AuthenticationCookieExpirationInMinutes must be greater than zero.")
    .ValidateOnStart();

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

builder.Services.AddSingleton<Localizer>();
builder.Services.AddScoped<AppState>();

builder.Services.AddHealthChecks();

builder.Services.AddHttpClient<AccountApiClient>(
    ConfigureApiClient);

builder.Services.AddHttpClient<AuthenticationApiClient>(
    ConfigureApiClient);

builder.Services.AddHttpClient<BillingApiClient>(
    ConfigureApiClient);

builder.Services.AddHttpClient<DigitalServicesApiClient>(
    ConfigureApiClient);

builder.Services.AddHttpClient<SavingsPlansApiClient>(
    ConfigureApiClient);

builder.Services.AddHttpClient<SubscriptionsApiClient>(
    ConfigureApiClient);

var app = builder.Build();

app.Use(async (context, next) =>
{
    context.Response.Headers.XContentTypeOptions =
        "nosniff";

    context.Response.Headers.XFrameOptions =
        "DENY";

    context.Response.Headers["Referrer-Policy"] =
        "no-referrer";

    context.Response.Headers["Permissions-Policy"] =
        "camera=(), microphone=(), geolocation=()";

    await next();
});

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

app.MapHealthChecks("/health");

app.MapStaticAssets();

app.MapAuthenticationEndpoints();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();

void ConfigureApiClient(
    IServiceProvider serviceProvider,
    HttpClient client)
{
    var options = serviceProvider
        .GetRequiredService<IOptions<ApiOptions>>()
        .Value;

    client.BaseAddress =
        new Uri(options.BaseUrl);

    client.Timeout =
        apiRequestTimeout;
}

public partial class Program;
