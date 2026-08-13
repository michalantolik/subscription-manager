using System.Security.Claims;
using System.Text.Json;
using System.Text.Json.Serialization;
using SubscriptionManager.Web.Features.Authentication.Security;

namespace SubscriptionManager.Web.Features.Billing;

/// <summary>
/// Provides access to billing-related API operations.
/// </summary>
public sealed class BillingApiClient(
    HttpClient httpClient)
{
    private static readonly JsonSerializerOptions JsonOptions =
        new(JsonSerializerDefaults.Web)
        {
            Converters =
            {
                new JsonStringEnumConverter()
            }
        };

    public async Task<IReadOnlyList<PaymentPlanPriceResponse>>
        GetPlansAsync(
            CancellationToken cancellationToken = default)
    {
        using var request =
            new HttpRequestMessage(
                HttpMethod.Get,
                "api/billing/plans");

        using var response =
            await httpClient.SendAsync(
                request,
                cancellationToken);

        response.EnsureSuccessStatusCode();

        return await response.Content
                   .ReadFromJsonAsync<
                       List<PaymentPlanPriceResponse>>(
                       JsonOptions,
                       cancellationToken)
               ?? [];
    }

    public async Task<BillingOverviewResponse> GetOverviewAsync(
        ClaimsPrincipal user,
        CancellationToken cancellationToken = default)
    {
        using var request =
            new HttpRequestMessage(
                HttpMethod.Get,
                "api/billing");

        ApiRequestAuthorization.AddBearerToken(
            request,
            user);

        using var response =
            await httpClient.SendAsync(
                request,
                cancellationToken);

        response.EnsureSuccessStatusCode();

        return await response.Content
                   .ReadFromJsonAsync<BillingOverviewResponse>(
                       JsonOptions,
                       cancellationToken)
               ?? throw new InvalidOperationException(
                   "The billing overview response was empty.");
    }

    public async Task<Uri> CreateCheckoutSessionAsync(
        BillingPlan plan,
        BillingInterval billingInterval,
        Uri successUrl,
        Uri cancelUrl,
        ClaimsPrincipal user,
        CancellationToken cancellationToken = default)
    {
        using var request =
            new HttpRequestMessage(
                HttpMethod.Post,
                "api/billing/checkout")
            {
                Content =
                    JsonContent.Create(
                        new
                        {
                            Plan = plan,
                            BillingInterval =
                                billingInterval,
                            SuccessUrl =
                                successUrl.ToString(),
                            CancelUrl =
                                cancelUrl.ToString()
                        },
                        options:
                            JsonOptions)
            };

        ApiRequestAuthorization.AddBearerToken(
            request,
            user);

        using var response =
            await httpClient.SendAsync(
                request,
                cancellationToken);

        response.EnsureSuccessStatusCode();

        var checkout =
            await response.Content
                .ReadFromJsonAsync<
                    CreateCheckoutSessionResponse>(
                    JsonOptions,
                    cancellationToken)
            ?? throw new InvalidOperationException(
                "The checkout session response was empty.");

        return new Uri(
            checkout.CheckoutUrl);
    }

    public async Task<SubscriptionChangePreviewResponse>
        PreviewChangeAsync(
            BillingPlan plan,
            BillingInterval billingInterval,
            ClaimsPrincipal user,
            CancellationToken cancellationToken = default)
    {
        using var request =
            CreatePlanChangeRequest(
                "api/billing/subscription/change-preview",
                plan,
                billingInterval);

        ApiRequestAuthorization.AddBearerToken(
            request,
            user);

        using var response =
            await httpClient.SendAsync(
                request,
                cancellationToken);

        response.EnsureSuccessStatusCode();

        return await response.Content
                   .ReadFromJsonAsync<
                       SubscriptionChangePreviewResponse>(
                       JsonOptions,
                       cancellationToken)
               ?? throw new InvalidOperationException(
                   "The subscription change preview response was empty.");
    }

    public async Task ChangeAsync(
        BillingPlan plan,
        BillingInterval billingInterval,
        ClaimsPrincipal user,
        CancellationToken cancellationToken = default)
    {
        using var request =
            CreatePlanChangeRequest(
                "api/billing/subscription/change",
                plan,
                billingInterval);

        ApiRequestAuthorization.AddBearerToken(
            request,
            user);

        using var response =
            await httpClient.SendAsync(
                request,
                cancellationToken);

        response.EnsureSuccessStatusCode();
    }

    public async Task CancelAsync(
        ClaimsPrincipal user,
        CancellationToken cancellationToken = default)
    {
        await SendEmptyPostAsync(
            "api/billing/subscription/cancel",
            user,
            cancellationToken);
    }

    public async Task ResumeAsync(
        ClaimsPrincipal user,
        CancellationToken cancellationToken = default)
    {
        await SendEmptyPostAsync(
            "api/billing/subscription/resume",
            user,
            cancellationToken);
    }

    private async Task SendEmptyPostAsync(
        string requestUri,
        ClaimsPrincipal user,
        CancellationToken cancellationToken)
    {
        using var request =
            new HttpRequestMessage(
                HttpMethod.Post,
                requestUri);

        ApiRequestAuthorization.AddBearerToken(
            request,
            user);

        using var response =
            await httpClient.SendAsync(
                request,
                cancellationToken);

        response.EnsureSuccessStatusCode();
    }

    private static HttpRequestMessage CreatePlanChangeRequest(
        string requestUri,
        BillingPlan plan,
        BillingInterval billingInterval)
    {
        return new HttpRequestMessage(
            HttpMethod.Post,
            requestUri)
        {
            Content =
                JsonContent.Create(
                    new
                    {
                        Plan = plan,
                        BillingInterval =
                            billingInterval
                    },
                    options:
                        JsonOptions)
        };
    }
}
