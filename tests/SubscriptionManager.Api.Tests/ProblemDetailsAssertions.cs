using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc;

namespace SubscriptionManager.Api.Tests;

public static class ProblemDetailsAssertions
{
    public static async Task AssertAsync(
        HttpResponseMessage response,
        HttpStatusCode expectedStatusCode,
        string expectedTitle,
        string expectedDetail,
        string expectedInstance)
    {
        var problemDetails = await ReadAsync(
            response,
            expectedStatusCode);

        Assert.Equal(expectedTitle, problemDetails.Title);
        Assert.Equal(expectedDetail, problemDetails.Detail);
        Assert.Equal(expectedInstance, problemDetails.Instance);
    }

    public static async Task AssertContainsAsync(
        HttpResponseMessage response,
        HttpStatusCode expectedStatusCode,
        string expectedTitle,
        string expectedDetailFragment,
        string expectedInstance)
    {
        var problemDetails = await ReadAsync(
            response,
            expectedStatusCode);

        Assert.Equal(expectedTitle, problemDetails.Title);
        Assert.Contains(
            expectedDetailFragment,
            problemDetails.Detail);
        Assert.Equal(expectedInstance, problemDetails.Instance);
    }

    private static async Task<ProblemDetails> ReadAsync(
        HttpResponseMessage response,
        HttpStatusCode expectedStatusCode)
    {
        Assert.Equal(expectedStatusCode, response.StatusCode);
        Assert.Equal(
            "application/problem+json",
            response.Content.Headers.ContentType?.MediaType);

        var problemDetails = await response.Content
            .ReadFromJsonAsync<ProblemDetails>();

        Assert.NotNull(problemDetails);
        Assert.Equal(
            (int)expectedStatusCode,
            problemDetails.Status);

        return problemDetails;
    }
}
