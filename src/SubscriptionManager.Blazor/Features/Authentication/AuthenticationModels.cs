using SubscriptionManager.Blazor.Features.Currencies;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Security.Claims;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SubscriptionManager.Blazor.Features.Authentication;

public sealed class RegisterFormModel
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MinLength(8)]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    [Required]
    [Compare(nameof(Password))]
    [DataType(DataType.Password)]
    public string ConfirmPassword { get; set; } = string.Empty;
}

public sealed class LoginFormModel
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;
}

public sealed class ForgotPasswordFormModel
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;
}

public sealed class ResetPasswordFormModel
{
    [Required]
    public Guid UserId { get; set; }

    [Required]
    public string ResetToken { get; set; } = string.Empty;

    [Required]
    [MinLength(8)]
    [DataType(DataType.Password)]
    public string NewPassword { get; set; } = string.Empty;

    [Required]
    [Compare(nameof(NewPassword))]
    [DataType(DataType.Password)]
    public string ConfirmPassword { get; set; } = string.Empty;
}

public sealed record AuthenticationError(
    string Code,
    string Description);

public sealed record AuthenticationOperationResult(
    bool Succeeded,
    IReadOnlyCollection<AuthenticationError> Errors)
{
    public static AuthenticationOperationResult Success()
        => new(true, []);

    public static AuthenticationOperationResult Failure(
        IEnumerable<AuthenticationError> errors)
        => new(false, errors.ToArray());
}

public sealed record LoginOperationResult(
    bool Succeeded,
    string? AccessToken,
    IReadOnlyCollection<AuthenticationError> Errors)
{
    public static LoginOperationResult Success(
        string accessToken)
        => new(
            true,
            accessToken,
            []);

    public static LoginOperationResult Failure(
        IEnumerable<AuthenticationError> errors)
        => new(
            false,
            null,
            errors.ToArray());
}

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
        string languageCode,
        CancellationToken cancellationToken = default)
    {
        using var response = await httpClient.PostAsJsonAsync(
            "api/auth/register",
            new
            {
                Email = email,
                Password = password,
                LanguageCode = languageCode
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
                loginResponse.AccessToken);
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

    public async Task<Currency?> GetBaseCurrencyAsync(
        ClaimsPrincipal user,
        CancellationToken cancellationToken = default)
    {
        using var request = new HttpRequestMessage(
            HttpMethod.Get,
            "api/auth/account/base-currency");

        ApiRequestAuthorization.AddBearerToken(
            request,
            user);

        using var response = await httpClient.SendAsync(
            request,
            cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        var result = await response.Content
            .ReadFromJsonAsync<BaseCurrencyResponse>(
                JsonOptions,
                cancellationToken);

        return result?.BaseCurrency;
    }

    public async Task<AuthenticationOperationResult> UpdateBaseCurrencyAsync(
        Currency baseCurrency,
        ClaimsPrincipal user,
        CancellationToken cancellationToken = default)
    {
        using var request = new HttpRequestMessage(
            HttpMethod.Put,
            "api/auth/account/base-currency")
        {
            Content = JsonContent.Create(
                new
                {
                    BaseCurrency = baseCurrency
                },
                options: JsonOptions)
        };

        ApiRequestAuthorization.AddBearerToken(
            request,
            user);

        using var response = await httpClient.SendAsync(
            request,
            cancellationToken);

        if (response.IsSuccessStatusCode)
        {
            return AuthenticationOperationResult.Success();
        }

        return await ReadFailureAsync(
            response,
            cancellationToken);
    }

    public async Task<AuthenticationOperationResult> DeleteAccountAsync(
        ClaimsPrincipal user,
        CancellationToken cancellationToken = default)
    {
        using var request = new HttpRequestMessage(
            HttpMethod.Delete,
            "api/auth/account");

        ApiRequestAuthorization.AddBearerToken(
            request,
            user);

        using var response = await httpClient.SendAsync(
            request,
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
        string AccessToken);

    private sealed record BaseCurrencyResponse(
        Currency BaseCurrency);

    private sealed record ValidationProblemResponse(
        IReadOnlyDictionary<string, string[]> Errors);
}
