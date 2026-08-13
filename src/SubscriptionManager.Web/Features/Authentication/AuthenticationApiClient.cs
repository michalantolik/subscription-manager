using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using SubscriptionManager.Web.Common.Currencies;
using SubscriptionManager.Web.Common.Localization;

namespace SubscriptionManager.Web.Features.Authentication;

/// <summary>
/// Provides access to authentication-related API operations.
/// </summary>
public sealed class AuthenticationApiClient(
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

    public async Task<AuthenticationOperationResult> RegisterAsync(
        string email,
        string password,
        Language language,
        Currency baseCurrency,
        CancellationToken cancellationToken = default)
    {
        using var response = await httpClient.PostAsJsonAsync(
            "api/auth/register",
            new
            {
                Email = email,
                Password = password,
                Language = language,
                BaseCurrency = baseCurrency
            },
            JsonOptions,
            cancellationToken);

        if (response.StatusCode == HttpStatusCode.Created)
        {
            return AuthenticationOperationResult.Success();
        }

        return await ReadFailureAsync(
            response,
            cancellationToken);
    }

    public async Task<AuthenticationOperationResult> ConfirmEmailAsync(
        Guid userId,
        string confirmationToken,
        CancellationToken cancellationToken = default)
    {
        using var response = await httpClient.PostAsJsonAsync(
            "api/auth/confirm-email",
            new
            {
                UserId = userId,
                ConfirmationToken = confirmationToken
            },
            JsonOptions,
            cancellationToken);

        if (response.IsSuccessStatusCode)
        {
            return AuthenticationOperationResult.Success();
        }

        return await ReadFailureAsync(
            response,
            cancellationToken);
    }

    public async Task<LoginOperationResult> LoginAsync(
        string email,
        string password,
        CancellationToken cancellationToken = default)
    {
        using var response = await httpClient.PostAsJsonAsync(
            "api/auth/login",
            new
            {
                Email = email,
                Password = password
            },
            JsonOptions,
            cancellationToken);

        if (response.IsSuccessStatusCode)
        {
            var loginResponse = await response.Content
                .ReadFromJsonAsync<LoginResponse>(
                    JsonOptions,
                    cancellationToken);

            if (string.IsNullOrWhiteSpace(loginResponse?.AccessToken))
            {
                return LoginOperationResult.Failure(
                [
                    UnexpectedError()
                ]);
            }

            return LoginOperationResult.Success(
                loginResponse.AccessToken,
                loginResponse.Language,
                loginResponse.SubscriptionPlan);
        }

        if (response.StatusCode is
            HttpStatusCode.BadRequest or
            HttpStatusCode.Unauthorized)
        {
            var errors = await ReadErrorsAsync(
                response,
                cancellationToken);

            return LoginOperationResult.Failure(errors);
        }

        return LoginOperationResult.Failure(
        [
            UnexpectedError()
        ]);
    }

    public async Task<AuthenticationOperationResult> ForgotPasswordAsync(
        string email,
        string languageCode,
        CancellationToken cancellationToken = default)
    {
        using var response = await httpClient.PostAsJsonAsync(
            "api/auth/forgot-password",
            new
            {
                Email = email,
                LanguageCode = languageCode
            },
            JsonOptions,
            cancellationToken);

        if (response.IsSuccessStatusCode)
        {
            return AuthenticationOperationResult.Success();
        }

        return await ReadFailureAsync(
            response,
            cancellationToken);
    }

    public async Task<AuthenticationOperationResult> ResetPasswordAsync(
        Guid userId,
        string resetToken,
        string newPassword,
        CancellationToken cancellationToken = default)
    {
        using var response = await httpClient.PostAsJsonAsync(
            "api/auth/reset-password",
            new
            {
                UserId = userId,
                ResetToken = resetToken,
                NewPassword = newPassword
            },
            JsonOptions,
            cancellationToken);

        if (response.IsSuccessStatusCode)
        {
            return AuthenticationOperationResult.Success();
        }

        return await ReadFailureAsync(
            response,
            cancellationToken);
    }

    private static async Task<AuthenticationOperationResult> ReadFailureAsync(
        HttpResponseMessage response,
        CancellationToken cancellationToken)
    {
        if (response.StatusCode != HttpStatusCode.BadRequest)
        {
            return UnexpectedFailure();
        }

        var errors = await ReadErrorsAsync(
            response,
            cancellationToken);

        return AuthenticationOperationResult.Failure(errors);
    }

    private static async Task<IReadOnlyCollection<AuthenticationError>>
        ReadErrorsAsync(
            HttpResponseMessage response,
            CancellationToken cancellationToken)
    {
        try
        {
            var problem = await response.Content
                .ReadFromJsonAsync<ValidationProblemResponse>(
                    JsonOptions,
                    cancellationToken);

            if (problem?.Errors is null ||
                problem.Errors.Count == 0)
            {
                return
                [
                    UnexpectedError()
                ];
            }

            return problem.Errors
                .SelectMany(pair =>
                    pair.Value.Select(description =>
                        new AuthenticationError(
                            pair.Key,
                            description)))
                .ToArray();
        }
        catch (JsonException)
        {
            return
            [
                UnexpectedError()
            ];
        }
    }

    private static AuthenticationOperationResult UnexpectedFailure()
        => AuthenticationOperationResult.Failure(
        [
            UnexpectedError()
        ]);

    private static AuthenticationError UnexpectedError()
        => new(
            "UnexpectedError",
            "The operation could not be completed.");

    private sealed record LoginResponse(
        string AccessToken,
        Language Language,
        string SubscriptionPlan);

    private sealed record ValidationProblemResponse(
        IReadOnlyDictionary<string, string[]> Errors);
}
