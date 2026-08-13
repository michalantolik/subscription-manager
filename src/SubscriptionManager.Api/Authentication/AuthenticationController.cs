using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using SubscriptionManager.Application.Account.DeleteAccount;
using SubscriptionManager.Application.Authentication.ConfirmEmail;
using SubscriptionManager.Application.Authentication.ForgotPassword;
using SubscriptionManager.Application.Authentication.LoginUser;
using SubscriptionManager.Application.Authentication.RegisterUser;
using SubscriptionManager.Application.Authentication.ResetPassword;
using SubscriptionManager.Application.Common.Identity;
using SubscriptionManager.Application.Common.Localization;
using SubscriptionManager.Domain.Billing;
using SubscriptionManager.Domain.Subscriptions;

namespace SubscriptionManager.Api.Authentication;

/// <summary>
/// Exposes authentication and identity-related use cases through HTTP endpoints.
/// </summary>
[ApiController]
[Route("api/auth")]
public sealed class AuthenticationController(
    RegisterUserHandler registerUserHandler,
    ConfirmEmailHandler confirmEmailHandler,
    LoginUserHandler loginUserHandler,
    ForgotPasswordHandler forgotPasswordHandler,
    ResetPasswordHandler resetPasswordHandler,
    DeleteAccountHandler deleteAccountHandler,
    ICurrentUser currentUser)
    : ControllerBase
{
    [EnableRateLimiting("register")]
    [HttpPost("register")]
    public async Task<ActionResult<RegisterUserResponse>> RegisterAsync(
        RegisterUserRequest request,
        CancellationToken cancellationToken)
    {
        var command = new RegisterUserCommand(
            request.Email,
            request.Password,
            request.Language,
            request.BaseCurrency);

        var result = await registerUserHandler.HandleAsync(
            command,
            cancellationToken);

        if (!result.Succeeded)
        {
            return ValidationProblem(
                CreateValidationProblemDetails(result.Errors));
        }

        var response = new RegisterUserResponse(
            result.UserId!.Value);

        return StatusCode(
            StatusCodes.Status201Created,
            response);
    }

    [HttpPost("confirm-email")]
    public async Task<IActionResult> ConfirmEmailAsync(
        ConfirmEmailRequest request,
        CancellationToken cancellationToken)
    {
        var command = new ConfirmEmailCommand(
            request.UserId,
            request.ConfirmationToken);

        var result = await confirmEmailHandler.HandleAsync(
            command,
            cancellationToken);

        if (!result.Succeeded)
        {
            return ValidationProblem(
                CreateValidationProblemDetails(result.Errors));
        }

        return NoContent();
    }

    [EnableRateLimiting("login")]
    [HttpPost("login")]
    public async Task<ActionResult<LoginUserResponse>> LoginAsync(
        LoginUserRequest request,
        CancellationToken cancellationToken)
    {
        var command = new LoginUserCommand(
            request.Email,
            request.Password);

        var result = await loginUserHandler.HandleAsync(
            command,
            cancellationToken);

        if (!result.Succeeded)
        {
            return ValidationProblem(
                CreateValidationProblemDetails(result.Errors));
        }

        var response = new LoginUserResponse(
            result.AccessToken!,
            result.Language!.Value,
            result.SubscriptionPlan!.Value);

        return Ok(response);
    }

    [EnableRateLimiting("forgot-password")]
    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPasswordAsync(
        ForgotPasswordRequest request,
        CancellationToken cancellationToken)
    {
        var command = new ForgotPasswordCommand(
            request.Email,
            request.LanguageCode);

        await forgotPasswordHandler.HandleAsync(
            command,
            cancellationToken);

        return NoContent();
    }

    [EnableRateLimiting("reset-password")]
    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPasswordAsync(
        ResetPasswordRequest request,
        CancellationToken cancellationToken)
    {
        var command = new ResetPasswordCommand(
            request.UserId,
            request.ResetToken,
            request.NewPassword);

        var result = await resetPasswordHandler.HandleAsync(
            command,
            cancellationToken);

        if (!result.Succeeded)
        {
            return ValidationProblem(
                CreateValidationProblemDetails(result.Errors));
        }

        return NoContent();
    }

    [Authorize]
    [HttpGet("me")]
    public ActionResult<CurrentUserResponse> GetCurrentUser()
    {
        return Ok(
            new CurrentUserResponse(
                currentUser.UserId));
    }

    [Authorize]
    [HttpDelete("account")]
    public async Task<IActionResult> DeleteAccountAsync(
        CancellationToken cancellationToken)
    {
        var command = new DeleteAccountCommand(
            currentUser.UserId);

        var result = await deleteAccountHandler.HandleAsync(
            command,
            cancellationToken);

        if (!result.Succeeded)
        {
            return ValidationProblem(
                CreateValidationProblemDetails(result.Errors));
        }

        return NoContent();
    }

    private static ValidationProblemDetails CreateValidationProblemDetails(
        IReadOnlyCollection<IdentityServiceError> errors)
    {
        var validationErrors = errors
            .GroupBy(error => error.Code)
            .ToDictionary(
                group => group.Key,
                group => group
                    .Select(error => error.Description)
                    .ToArray());

        return new ValidationProblemDetails(
            validationErrors);
    }
}

/// <summary>
/// Registration data accepted by the API.
/// </summary>
public sealed record RegisterUserRequest(
    string Email,
    string Password,
    Language Language,
    Currency BaseCurrency);

/// <summary>
/// Registration data returned by the API.
/// </summary>
public sealed record RegisterUserResponse(
    Guid UserId);

/// <summary>
/// Email confirmation data accepted by the API.
/// </summary>
public sealed record ConfirmEmailRequest(
    Guid UserId,
    string ConfirmationToken);

/// <summary>
/// Login data accepted by the API.
/// </summary>
public sealed record LoginUserRequest(
    string Email,
    string Password);

/// <summary>
/// Authentication data returned by the API.
/// </summary>
public sealed record LoginUserResponse(
    string AccessToken,
    Language Language,
    SubscriptionPlan SubscriptionPlan);

/// <summary>
/// Password recovery data accepted by the API.
/// </summary>
public sealed record ForgotPasswordRequest(
    string Email,
    string LanguageCode);

/// <summary>
/// Password reset data accepted by the API.
/// </summary>
public sealed record ResetPasswordRequest(
    Guid UserId,
    string ResetToken,
    string NewPassword);

/// <summary>
/// Current user data returned by the API.
/// </summary>
public sealed record CurrentUserResponse(
    Guid UserId);
