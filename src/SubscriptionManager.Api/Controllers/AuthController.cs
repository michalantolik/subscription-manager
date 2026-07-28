using Microsoft.AspNetCore.Mvc;
using SubscriptionManager.Application.Identity.RegisterUser;

namespace SubscriptionManager.Api.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController(
    RegisterUserHandler registerUserHandler)
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
            var errors = result.Errors
                .GroupBy(error => error.Code)
                .ToDictionary(
                    group => group.Key,
                    group => group
                        .Select(error => error.Description)
                        .ToArray());

            return ValidationProblem(
                new ValidationProblemDetails(errors));
        }

        var response = new RegisterUserResponse(
            result.UserId!.Value);

        return StatusCode(
            StatusCodes.Status201Created,
            response);
    }
}

public sealed record RegisterUserRequest(
    string Email,
    string Password);

public sealed record RegisterUserResponse(
    Guid UserId);
