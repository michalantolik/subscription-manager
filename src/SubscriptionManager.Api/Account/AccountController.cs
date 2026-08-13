using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SubscriptionManager.Application.Account.GetAccountPreferences;
using SubscriptionManager.Application.Account.UpdateAccountPreferences;
using SubscriptionManager.Application.Common.Identity;
using SubscriptionManager.Application.Common.Localization;
using SubscriptionManager.Domain.Subscriptions;

namespace SubscriptionManager.Api.Account;

/// <summary>
/// Exposes account preference use cases through HTTP endpoints.
/// </summary>
[ApiController]
[Authorize]
[Route("api/account")]
public sealed class AccountController(
    GetAccountPreferencesHandler getAccountPreferencesHandler,
    UpdateAccountPreferencesHandler updateAccountPreferencesHandler,
    ICurrentUser currentUser)
    : ControllerBase
{
    [HttpGet("preferences")]
    public async Task<ActionResult<AccountPreferencesResponse>>
        GetPreferencesAsync(
            CancellationToken cancellationToken)
    {
        var preferences =
            await getAccountPreferencesHandler.HandleAsync(
                currentUser.UserId,
                cancellationToken);

        if (preferences is null)
        {
            return NotFound();
        }

        return Ok(
            new AccountPreferencesResponse(
                preferences.Language,
                preferences.BaseCurrency));
    }

    [HttpPut("preferences")]
    public async Task<IActionResult> UpdatePreferencesAsync(
        UpdateAccountPreferencesRequest request,
        CancellationToken cancellationToken)
    {
        var command =
            new UpdateAccountPreferencesCommand(
                currentUser.UserId,
                request.Language,
                request.BaseCurrency);

        var updated =
            await updateAccountPreferencesHandler.HandleAsync(
                command,
                cancellationToken);

        if (!updated)
        {
            return NotFound();
        }

        return NoContent();
    }
}

/// <summary>
/// Account preference data returned by the API.
/// </summary>
public sealed record AccountPreferencesResponse(
    Language Language,
    Currency BaseCurrency);

/// <summary>
/// Account preference data accepted by the API.
/// </summary>
public sealed record UpdateAccountPreferencesRequest(
    Language Language,
    Currency BaseCurrency);
