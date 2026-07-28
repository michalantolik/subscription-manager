using System.IdentityModel.Tokens.Jwt;
using SubscriptionManager.Application.Common.Identity;

namespace SubscriptionManager.Api.Authentication;

public sealed class CurrentUser(
    IHttpContextAccessor httpContextAccessor)
    : ICurrentUser
{
    public Guid UserId
    {
        get
        {
            var userIdValue = httpContextAccessor
                .HttpContext?
                .User
                .FindFirst(JwtRegisteredClaimNames.Sub)?
                .Value;

            if (!Guid.TryParse(userIdValue, out var userId)
                || userId == Guid.Empty)
            {
                throw new InvalidOperationException(
                    "The current user identifier is unavailable.");
            }

            return userId;
        }
    }
}
