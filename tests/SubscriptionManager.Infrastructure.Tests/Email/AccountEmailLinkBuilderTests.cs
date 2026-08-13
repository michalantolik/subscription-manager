using Microsoft.Extensions.Options;
using SubscriptionManager.Infrastructure.Authentication.Email;

namespace SubscriptionManager.Infrastructure.Tests.Email;

public sealed class AccountEmailLinkBuilderTests
{
    private const string ApplicationBaseUrl =
        "https://subscription-manager.example.com";

    [Fact]
    public void BuildEmailConfirmationLink_ShouldBuildExpectedLink()
    {
        var builder = CreateBuilder();

        var userId =
            Guid.Parse(
                "11111111-2222-3333-4444-555555555555");

        const string token =
            "confirmation+token/value==";

        var link =
            builder.BuildEmailConfirmationLink(
                userId,
                token);

        Assert.StartsWith(
            $"{ApplicationBaseUrl}/confirm-email?",
            link);

        Assert.Contains(
            $"userId={userId}",
            link);

        Assert.Contains(
            "token=confirmation%2Btoken%2Fvalue%3D%3D",
            link);
    }

    [Fact]
    public void BuildPasswordResetLink_ShouldBuildExpectedLink()
    {
        var builder = CreateBuilder();

        var userId =
            Guid.Parse(
                "11111111-2222-3333-4444-555555555555");

        const string token =
            "reset+token/value==";

        var link =
            builder.BuildPasswordResetLink(
                userId,
                token);

        Assert.StartsWith(
            $"{ApplicationBaseUrl}/reset-password?",
            link);

        Assert.Contains(
            $"userId={userId}",
            link);

        Assert.Contains(
            "token=reset%2Btoken%2Fvalue%3D%3D",
            link);
    }

    [Fact]
    public void BuildEmailConfirmationLink_ShouldHandleTrailingSlash()
    {
        var options =
            Options.Create(
                new EmailOptions
                {
                    ApplicationBaseUrl =
                        $"{ApplicationBaseUrl}/"
                });

        var builder =
            new AccountEmailLinkBuilder(options);

        var link =
            builder.BuildEmailConfirmationLink(
                Guid.NewGuid(),
                "token");

        Assert.StartsWith(
            $"{ApplicationBaseUrl}/confirm-email?",
            link);

        Assert.DoesNotContain(
            ".com//confirm-email",
            link);
    }

    private static AccountEmailLinkBuilder CreateBuilder()
    {
        var options =
            Options.Create(
                new EmailOptions
                {
                    ApplicationBaseUrl =
                        ApplicationBaseUrl
                });

        return new AccountEmailLinkBuilder(options);
    }
}
