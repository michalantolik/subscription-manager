using System.Net;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text.Json;
using SubscriptionManager.Web.Features.Authentication.Security;
using SubscriptionManager.Web.Features.DigitalServices;

namespace SubscriptionManager.Web.Tests;

public sealed class DigitalServicesApiClientTests
{
    [Fact]
    public async Task CreateAsync_SendsAuthorizedCustomServiceRequest()
    {
        CapturedRequest? capturedRequest = null;
        var digitalServiceId = Guid.NewGuid();

        using var httpClient = new HttpClient(
            new StubHttpMessageHandler(request =>
            {
                capturedRequest = new CapturedRequest(
                    request.Method,
                    request.RequestUri,
                    request.Headers.Authorization?.Scheme,
                    request.Headers.Authorization?.Parameter,
                    request.Content?.ReadAsStringAsync()
                        .GetAwaiter()
                        .GetResult());

                return new HttpResponseMessage(
                    HttpStatusCode.Created)
                {
                    Content = JsonContent.Create(digitalServiceId)
                };
            }))
        {
            BaseAddress = new Uri("https://api.example.com")
        };

        var apiClient = new DigitalServicesApiClient(
            httpClient);

        var user = new ClaimsPrincipal(
            new ClaimsIdentity(
            [
                new Claim(
                    AuthenticationClaimTypes.AccessToken,
                    "access-token")
            ],
            "Test"));

        var result = await apiClient.CreateAsync(
            new CreateDigitalServiceFormModel
            {
                Name = " Custom service ",
                Category = " Productivity ",
                ManagementUrl = " https://example.com/account "
            },
            user);

        Assert.Equal(digitalServiceId, result);
        Assert.NotNull(capturedRequest);
        Assert.Equal(HttpMethod.Post, capturedRequest.Method);
        Assert.Equal(
            "https://api.example.com/api/digital-services",
            capturedRequest.RequestUri?.ToString());
        Assert.Equal("Bearer", capturedRequest.AuthorizationScheme);
        Assert.Equal("access-token", capturedRequest.AccessToken);

        using var document = JsonDocument.Parse(
            capturedRequest.Body!);

        var root = document.RootElement;

        Assert.StartsWith(
            "custom-",
            root.GetProperty("key").GetString()!);
        Assert.Equal(
            "Custom service",
            root.GetProperty("name").GetString());
        Assert.Equal(
            "Other",
            root.GetProperty("category").GetString());
        Assert.Equal(
            "Productivity",
            root.GetProperty("customCategoryName").GetString());
        Assert.Equal(
            "https://example.com/account",
            root.GetProperty("managementUrl").GetString());
    }

    private sealed record CapturedRequest(
        HttpMethod Method,
        Uri? RequestUri,
        string? AuthorizationScheme,
        string? AccessToken,
        string? Body);

    private sealed class StubHttpMessageHandler(
        Func<HttpRequestMessage, HttpResponseMessage> responseFactory)
        : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            return Task.FromResult(
                responseFactory(request));
        }
    }
}
