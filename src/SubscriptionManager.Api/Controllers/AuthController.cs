using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SubscriptionManager.Application.Common.Identity;
using SubscriptionManager.Application.Identity.ConfirmEmail;
using SubscriptionManager.Application.Identity.DeleteUser;
using SubscriptionManager.Application.Identity.ForgotPassword;
using SubscriptionManager.Application.Identity.GetBaseCurrency;
using SubscriptionManager.Application.Identity.LoginUser;
using SubscriptionManager.Application.Identity.RegisterUser;
using SubscriptionManager.Application.Identity.ResetPassword;
using SubscriptionManager.Application.Identity.UpdateBaseCurrency;
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
    GetBaseCurrencyHandler getBaseCurrencyHandler,
    UpdateBaseCurrencyHandler updateBaseCurrencyHandler,
    DeleteUserHandler deleteUserHandler,
    ICurrentUser currentUser)
    : ControllerBase
{
    [HttpPost("register")]
    public async Task<ActionResult<RegisterUserResponse>> RegisterAsync(
        RegisterUserRequest request,
        CancellationToken cancellationToken)
    {
        var command = new RegisterUserCommand(
            request.Email,
            request.Password,
            request.LanguageCode);

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
            result.AccessToken!);

        return Ok(response);
    }

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
    [HttpGet("account/base-currency")]
    public async Task<ActionResult<BaseCurrencyResponse>>
        GetBaseCurrencyAsync(
            CancellationToken cancellationToken)
    {
        var baseCurrency =
            await getBaseCurrencyHandler.HandleAsync(
                currentUser.UserId,
                cancellationToken);

        if (baseCurrency is null)
        {
            return NotFound();
        }

        return Ok(
            new BaseCurrencyResponse(
                baseCurrency.Value));
    }

    [Authorize]
    [HttpPut("account/base-currency")]
    public async Task<IActionResult> UpdateBaseCurrencyAsync(
        UpdateBaseCurrencyRequest request,
        CancellationToken cancellationToken)
    {
        var command =
            new UpdateBaseCurrencyCommand(
                currentUser.UserId,
                request.BaseCurrency);

        var updated =
            await updateBaseCurrencyHandler.HandleAsync(
                command,
                cancellationToken);

        if (!updated)
        {
            return NotFound();
        }

        return NoContent();
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
    string LanguageCode);

public sealed record RegisterUserResponse(
    Guid UserId);

public sealed record ConfirmEmailRequest(
    Guid UserId,
    string ConfirmationToken);

public sealed record LoginUserRequest(
    string Email,
    string Password);

public sealed record LoginUserResponse(
    string AccessToken);

public sealed record ForgotPasswordRequest(
    string Email,
    string LanguageCode);

public sealed record ResetPasswordRequest(
    Guid UserId,
    string ResetToken,
    string NewPassword);

public sealed record CurrentUserResponse(
    Guid UserId);

public sealed record BaseCurrencyResponse(
    Currency BaseCurrency);

public sealed record UpdateBaseCurrencyRequest(
    Currency BaseCurrency);
