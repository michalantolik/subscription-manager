using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SubscriptionManager.Application.Common.Identity;
using SubscriptionManager.Application.Identity.ConfirmEmail;
using SubscriptionManager.Application.Identity.LoginUser;
using SubscriptionManager.Application.Identity.RegisterUser;

namespace SubscriptionManager.Api.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController(
    RegisterUserHandler registerUserHandler,
    ConfirmEmailHandler confirmEmailHandler,
    LoginUserHandler loginUserHandler,
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
            request.Password);

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

    [Authorize]
    [HttpGet("me")]
    public ActionResult<CurrentUserResponse> GetCurrentUser()
    {
        return Ok(
            new CurrentUserResponse(currentUser.UserId));
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

        return new ValidationProblemDetails(validationErrors);
    }
}

public sealed record RegisterUserRequest(
    string Email,
    string Password);

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

public sealed record CurrentUserResponse(
    Guid UserId);
