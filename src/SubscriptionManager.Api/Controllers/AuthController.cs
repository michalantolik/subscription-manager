using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using SubscriptionManager.Application.Common.Identity;
using SubscriptionManager.Application.Common.Localization;
using SubscriptionManager.Application.Identity.ConfirmEmail;
using SubscriptionManager.Application.Identity.DeleteUser;
using SubscriptionManager.Application.Identity.ForgotPassword;
using SubscriptionManager.Application.Identity.LoginUser;
using SubscriptionManager.Application.Identity.RegisterUser;
using SubscriptionManager.Application.Identity.ResetPassword;
using SubscriptionManager.Domain.Subscriptions;

namespace SubscriptionManager.Api.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController(
    RegisterUserHandler registerUserHandler,
    ConfirmEmailHandler confirmEmailHandler,
    LoginUserHandler loginUserHandler,
    ForgotPasswordHandler forgotPasswordHandler,
    ResetPasswordHandler resetPasswordHandler,
    DeleteUserHandler deleteUserHandler,
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
        var command = new DeleteUserCommand(
            currentUser.UserId);

        var result = await deleteUserHandler.HandleAsync(
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

public sealed record RegisterUserRequest(
    string Email,
    string Password,
    Language Language,
    Currency BaseCurrency);

public sealed record RegisterUserResponse(
    Guid UserId);

public sealed record ConfirmEmailRequest(
    Guid UserId,
    string ConfirmationToken);

public sealed record LoginUserRequest(
    string Email,
    string Password);

public sealed record LoginUserResponse(
    string AccessToken,
    Language Language,
    SubscriptionPlan SubscriptionPlan);

public sealed record ForgotPasswordRequest(
    string Email,
    string LanguageCode);

public sealed record ResetPasswordRequest(
    Guid UserId,
    string ResetToken,
    string NewPassword);

public sealed record CurrentUserResponse(
    Guid UserId);
