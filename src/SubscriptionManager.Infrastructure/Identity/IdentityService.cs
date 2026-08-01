using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SubscriptionManager.Application.Common.Identity;
using SubscriptionManager.Infrastructure.Persistence;

namespace SubscriptionManager.Infrastructure.Identity;

public sealed class IdentityService(
    UserManager<ApplicationUser> userManager,
    SubscriptionManagerDbContext dbContext)
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

    public async Task<AuthenticateUserResult> AuthenticateUserAsync(
        string email,
        string password,
        CancellationToken cancellationToken = default)
    {
        var user = await userManager.FindByEmailAsync(email);

        if (user is null)
        {
            return AuthenticationFailed();
        }

        if (!user.EmailConfirmed)
        {
            return AuthenticateUserResult.Failure(
            [
                new IdentityServiceError(
                    "EmailNotConfirmed",
                    "The email address has not been confirmed.")
            ]);
        }

        var passwordIsValid = await userManager.CheckPasswordAsync(
            user,
            password);

        if (!passwordIsValid)
        {
            return AuthenticationFailed();
        }

        return AuthenticateUserResult.Success(user.Id);
    }

    public async Task<PasswordResetToken?> GeneratePasswordResetTokenAsync(
        string email,
        CancellationToken cancellationToken = default)
    {
        var user = await userManager.FindByEmailAsync(email);

        if (user is null)
        {
            return null;
        }

        var token = await userManager.GeneratePasswordResetTokenAsync(user);

        return new PasswordResetToken(
            user.Id,
            user.Email!,
            token);
    }

    public async Task<ResetPasswordResult> ResetPasswordAsync(
        Guid userId,
        string resetToken,
        string newPassword,
        CancellationToken cancellationToken = default)
    {
        var user = await userManager.FindByIdAsync(userId.ToString());

        if (user is null)
        {
            return ResetPasswordResult.Failure(
            [
                new IdentityServiceError(
                    "UserNotFound",
                    "The user was not found.")
            ]);
        }

        var result = await userManager.ResetPasswordAsync(
            user,
            resetToken,
            newPassword);

        if (result.Succeeded)
        {
            return ResetPasswordResult.Success();
        }

        return ResetPasswordResult.Failure(
            result.Errors.Select(MapError));
    }

    public async Task<DeleteUserResult> DeleteUserAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var user = await userManager.FindByIdAsync(userId.ToString());

        if (user is null)
        {
            return DeleteUserResult.Failure(
            [
                new IdentityServiceError(
                    "UserNotFound",
                    "The user was not found.")
            ]);
        }

        await using var transaction =
            await dbContext.Database.BeginTransactionAsync(
                cancellationToken);

        await dbContext.Subscriptions
            .Where(subscription =>
                subscription.OwnerId == userId)
            .ExecuteDeleteAsync(cancellationToken);

        await dbContext.DigitalServices
            .Where(digitalService =>
                !digitalService.IsPredefined &&
                digitalService.OwnerId == userId)
            .ExecuteDeleteAsync(cancellationToken);

        var result = await userManager.DeleteAsync(user);

        if (!result.Succeeded)
        {
            await transaction.RollbackAsync(cancellationToken);

            return DeleteUserResult.Failure(
                result.Errors.Select(MapError));
        }

        await transaction.CommitAsync(cancellationToken);

        return DeleteUserResult.Success();
    }

    private static AuthenticateUserResult AuthenticationFailed()
    {
        return AuthenticateUserResult.Failure(
        [
            new IdentityServiceError(
                "InvalidCredentials",
                "The email address or password is invalid.")
        ]);
    }

    private static IdentityServiceError MapError(
        IdentityError error)
    {
        return new IdentityServiceError(
            error.Code,
            error.Description);
    }
}
