using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.Options;
using SubscriptionManager.Blazor.Components;
using SubscriptionManager.Blazor.Configuration;
using SubscriptionManager.Blazor.Features.Subscriptions;
using SubscriptionManager.Blazor.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents().AddInteractiveServerComponents(options =>
{
    options.DetailedErrors = builder.Environment.IsDevelopment();
});

builder.Services.Configure<ApiOptions>(builder.Configuration.GetSection(ApiOptions.SectionName));
builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    options.DefaultRequestCulture = new RequestCulture(SupportedCultures.DefaultCulture);
    options.SupportedCultures = SupportedCultures.All.ToList();
    options.SupportedUICultures = SupportedCultures.All.ToList();
    options.RequestCultureProviders = [new CookieRequestCultureProvider()];
});

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<AppState>();
builder.Services.AddScoped<Localizer>();
builder.Services.AddHttpClient<SubscriptionsApiClient>((serviceProvider, client) =>
{
    var options = serviceProvider.GetRequiredService<IOptions<ApiOptions>>().Value;
    client.BaseAddress = new Uri(options.BaseUrl);
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();
app.UseRequestLocalization();
app.UseAntiforgery();

app.MapGet("/culture/set", (string culture, string? redirectUri, HttpContext context) =>
{
    var selectedCulture = SupportedCultures.Contains(culture) ? culture : SupportedCultures.DefaultCulture;
    context.Response.Cookies.Append(
        CookieRequestCultureProvider.DefaultCookieName,
        CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(selectedCulture)),
        new CookieOptions { Expires = DateTimeOffset.UtcNow.AddYears(1), IsEssential = true, SameSite = SameSiteMode.Lax, Secure = context.Request.IsHttps });

    var safeRedirect = !string.IsNullOrWhiteSpace(redirectUri) && redirectUri.StartsWith('/') && !redirectUri.StartsWith("//") ? redirectUri : "/";
    return Results.LocalRedirect(safeRedirect);
});

app.MapStaticAssets();
app.MapRazorComponents<App>().AddInteractiveServerRenderMode();
app.Run();
