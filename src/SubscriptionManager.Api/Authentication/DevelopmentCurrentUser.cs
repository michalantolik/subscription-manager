using SubscriptionManager.Application.Common.Identity;

namespace SubscriptionManager.Api.Authentication;

public sealed class DevelopmentCurrentUser : ICurrentUser
{
    private static readonly Guid MichalId =
        Guid.Parse("11111111-1111-1111-1111-111111111111");

    private static readonly Guid MarcinId =
        Guid.Parse("22222222-2222-2222-2222-222222222222");

    private const string UserIdHeaderName =
        "X-SubscriptionManager-User-Id";

    private readonly IHttpContextAccessor _httpContextAccessor;

    public DevelopmentCurrentUser(
        IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid UserId
    {
        get
        {
            var headerValue = _httpContextAccessor
                .HttpContext?
                .Request
                .Headers[UserIdHeaderName]
                .FirstOrDefault();

            if (!Guid.TryParse(headerValue, out var userId))
            {
                return MichalId;
            }

            return userId == MichalId || userId == MarcinId
                ? userId
                : MichalId;
        }
    }
}
