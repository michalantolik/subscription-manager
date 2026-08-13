using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SubscriptionManager.Application.Account;
using SubscriptionManager.Application.Common.Identity;
using SubscriptionManager.Application.Common.Localization;
using SubscriptionManager.Domain.Billing;
using SubscriptionManager.Domain.Subscriptions;
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
        Language language,
        Currency baseCurrency,
        CancellationToken cancellationToken = default)
    {
        if (!Enum.IsDefined(language))
        {
            throw new ArgumentOutOfRangeException(
                nameof(language),
                "The language is not supported.");
        }

        if (!Enum.IsDefined(baseCurrency))
        {
            throw new ArgumentOutOfRangeException(
                nameof(baseCurrency),
                "The base currency is not supported.");
        }

        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = email,
            Email = email,
            Language = language,
            BaseCurrency = baseCurrency
        };

        var result = await userManager.CreateAsync(
            user,
            password);

        if (result.Succeeded)
        {
            return CreateUserResult.Success(user.Id);
        }

        return CreateUserResult.Failure(
            result.Errors.Select(MapError));
    }

    public async Task<AccountPreferences?> GetAccountPreferencesAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        return await dbContext.Users
            .AsNoTracking()
            .Where(user => user.Id == userId)
            .Select(user =>
                new AccountPreferences(
                    user.Language,
                    user.BaseCurrency))
            .SingleOrDefaultAsync(
                cancellationToken);
    }

    public async Task<Currency?> GetBaseCurrencyAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        return await dbContext.Users
            .AsNoTracking()
            .Where(user => user.Id == userId)
            .Select(user =>
                (Currency?)user.BaseCurrency)
            .SingleOrDefaultAsync(
                cancellationToken);
    }

    public async Task<string?> GetEmailAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        return await dbContext.Users
            .AsNoTracking()
            .Where(user => user.Id == userId)
            .Select(user => user.Email)
            .SingleOrDefaultAsync(
                cancellationToken);
    }

    public async Task<SubscriptionPlan?> GetSubscriptionPlanAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var userExists = await dbContext.Users
            .AsNoTracking()
            .AnyAsync(
                user => user.Id == userId,
                cancellationToken);

        if (!userExists)
        {
            return null;
        }

        var billingSubscription =
            await dbContext.BillingSubscriptions
                .AsNoTracking()
                .SingleOrDefaultAsync(
                    subscription =>
                        subscription.UserId == userId,
                    cancellationToken);

        if (billingSubscription is null)
        {
            return SubscriptionPlan.Free;
        }

        return billingSubscription.GrantsPaidAccessAt(
            DateTimeOffset.UtcNow)
                ? billingSubscription.Plan
                : SubscriptionPlan.Free;
    }

    public async Task<bool> UpdateAccountPreferencesAsync(
        Guid userId,
        Language language,
        Currency baseCurrency,
        CancellationToken cancellationToken = default)
    {
        if (!Enum.IsDefined(language))
        {
            throw new ArgumentOutOfRangeException(
                nameof(language),
                "The language is not supported.");
        }

        if (!Enum.IsDefined(baseCurrency))
        {
            throw new ArgumentOutOfRangeException(
                nameof(baseCurrency),
                "The base currency is not supported.");
        }

        var user = await dbContext.Users
            .SingleOrDefaultAsync(
                currentUser => currentUser.Id == userId,
                cancellationToken);

        if (user is null)
        {
            return false;
        }

        user.Language = language;
        user.BaseCurrency = baseCurrency;

        await dbContext.SaveChangesAsync(
            cancellationToken);

        return true;
    }

    public async Task<string?> GenerateEmailConfirmationTokenAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var user = await userManager.FindByIdAsync(
            userId.ToString());

        if (user is null)
        {
            return null;
        }

        return await userManager.GenerateEmailConfirmationTokenAsync(
            user);
    }

    public async Task<ConfirmEmailResult> ConfirmEmailAsync(
        Guid userId,
        string confirmationToken,
        CancellationToken cancellationToken = default)
    {
        var user = await userManager.FindByIdAsync(
            userId.ToString());

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

        if (await userManager.IsLockedOutAsync(user))
        {
            return AuthenticationFailed();
        }

        var passwordIsValid =
            await userManager.CheckPasswordAsync(
                user,
                password);

        if (!passwordIsValid)
        {
            await userManager.AccessFailedAsync(user);

            return AuthenticationFailed();
        }

        await userManager.ResetAccessFailedCountAsync(user);

        if (!user.EmailConfirmed)
        {
            return AuthenticateUserResult.Failure(
            [
                new IdentityServiceError(
                    "EmailNotConfirmed",
                    "The email address has not been confirmed.")
            ]);
        }

        var subscriptionPlan =
            await GetSubscriptionPlanAsync(
                user.Id,
                cancellationToken);

        if (subscriptionPlan is null)
        {
            return AuthenticationFailed();
        }

        return AuthenticateUserResult.Success(
            user.Id,
            user.Language,
            subscriptionPlan.Value);
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

        var token =
            await userManager.GeneratePasswordResetTokenAsync(
                user);

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
        var user = await userManager.FindByIdAsync(
            userId.ToString());

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
        var user = await userManager.FindByIdAsync(
            userId.ToString());

        if (user is null)
        {
            return DeleteUserResult.Failure(
            [
                new IdentityServiceError(
                    "UserNotFound",
                    "The user was not found.")
            ]);
        }

        var billingSubscription =
            await dbContext.BillingSubscriptions
                .AsNoTracking()
                .SingleOrDefaultAsync(
                    subscription =>
                        subscription.UserId == userId,
                    cancellationToken);

        if (billingSubscription?.PreventsAccountDeletion()
            == true)
        {
            return DeleteUserResult.Failure(
            [
                new IdentityServiceError(
                    "BillingSubscriptionActive",
                    "The billing subscription must end before the account can be deleted.")
            ]);
        }

        await using var transaction =
            await dbContext.Database.BeginTransactionAsync(
                cancellationToken);

        await dbContext.Subscriptions
            .Where(subscription =>
                subscription.OwnerId == userId)
            .ExecuteDeleteAsync(
                cancellationToken);

        await dbContext.SavingsPlanUsages
            .Where(usage =>
                usage.UserId == userId)
            .ExecuteDeleteAsync(
                cancellationToken);

        await dbContext.DigitalServices
            .Where(digitalService =>
                !digitalService.IsPredefined &&
                digitalService.OwnerId == userId)
            .ExecuteDeleteAsync(
                cancellationToken);

        var result = await userManager.DeleteAsync(user);

        if (!result.Succeeded)
        {
            await transaction.RollbackAsync(
                cancellationToken);

            return DeleteUserResult.Failure(
                result.Errors.Select(MapError));
        }

        await transaction.CommitAsync(
            cancellationToken);

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
