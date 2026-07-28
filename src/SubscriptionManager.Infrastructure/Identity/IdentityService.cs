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
        cancellationToken.ThrowIfCancellationRequested();

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

        var errors = result.Errors.Select(error =>
            new IdentityServiceError(
                error.Code,
                error.Description));

        return CreateUserResult.Failure(errors);
    }
}
