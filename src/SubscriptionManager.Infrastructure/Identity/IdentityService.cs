using Microsoft.AspNetCore.Identity;
using SubscriptionManager.Application.Common.Identity;

namespace SubscriptionManager.Infrastructure.Identity;

public sealed class IdentityService(
    UserManager<ApplicationUser> userManager)
    : IIdentityService
{
    public async Task<CreateUserResult> CreateUserAsync(
        string email,
        string password,
        CancellationToken cancellationToken = default)
    {
        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = email,
            Email = email
        };

        var result = await userManager.CreateAsync(user, password);

        if (result.Succeeded)
        {
            return CreateUserResult.Success(user.Id);
        }

        return CreateUserResult.Failure(
            result.Errors.Select(MapError));
    }

    public async Task<string?> GenerateEmailConfirmationTokenAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var user = await userManager.FindByIdAsync(userId.ToString());

        if (user is null)
        {
            return null;
        }

        return await userManager.GenerateEmailConfirmationTokenAsync(user);
    }

    public async Task<ConfirmEmailResult> ConfirmEmailAsync(
        Guid userId,
        string confirmationToken,
        CancellationToken cancellationToken = default)
    {
        var user = await userManager.FindByIdAsync(userId.ToString());

        if (user is null)
        {
            return ConfirmEmailResult.Failure(
            [
                new IdentityServiceError(
                    "UserNotFound",
                    "The user was not found.")
            ]);
        }

        var result = await userManager.ConfirmEmailAsync(
            user,
            confirmationToken);

        if (result.Succeeded)
        {
            return ConfirmEmailResult.Success();
        }

        return ConfirmEmailResult.Failure(
            result.Errors.Select(MapError));
    }

    private static IdentityServiceError MapError(
        IdentityError error)
    {
        return new IdentityServiceError(
            error.Code,
            error.Description);
    }
}
