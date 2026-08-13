using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;

namespace SubscriptionManager.Infrastructure.Authentication.Email;

/// <summary>
/// Builds account-related email links.
/// </summary>
public sealed class AccountEmailLinkBuilder(
    IOptions<EmailOptions> options)
{
    private readonly EmailOptions _options = options.Value;

    public string BuildEmailConfirmationLink(
        Guid userId,
        string confirmationToken)
    {
        return BuildLink(
            "/confirm-email",
            userId,
            confirmationToken);
    }

    public string BuildPasswordResetLink(
        Guid userId,
        string resetToken)
    {
        return BuildLink(
            "/reset-password",
            userId,
            resetToken);
    }

    private string BuildLink(
        string path,
        Guid userId,
        string token)
    {
        var baseUrl =
            _options.ApplicationBaseUrl.TrimEnd('/');

        var url = $"{baseUrl}{path}";

        return QueryHelpers.AddQueryString(
            url,
            new Dictionary<string, string?>
            {
                ["userId"] = userId.ToString(),
                ["token"] = token
            });
    }
}
