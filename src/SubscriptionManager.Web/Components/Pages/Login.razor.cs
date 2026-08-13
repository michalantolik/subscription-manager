using Microsoft.AspNetCore.Components;
using SubscriptionManager.Web.Features.Authentication;

namespace SubscriptionManager.Web.Components.Pages;

public partial class Login
{
    [SupplyParameterFromQuery(Name = "error")]
    private string? Error { get; set; }

    [SupplyParameterFromQuery(Name = "returnUrl")]
    private string? ReturnUrl { get; set; }

    [SupplyParameterFromQuery(Name = "status")]
    private string? Status { get; set; }


    private string ErrorMessage => Error switch
    {
        AuthenticationErrorCodes.Required =>
            T["Authentication.Login.Required"],

        AuthenticationErrorCodes.EmailNotConfirmed =>
            T["Authentication.Login.EmailNotConfirmed"],

        AuthenticationErrorCodes.ServiceUnavailable =>
            T["Authentication.Login.Unavailable"],

        AuthenticationErrorCodes.SessionExpired =>
            T["Authentication.Login.SessionExpired"],

        _ =>
            T["Authentication.Login.Error"]
    };

    private string GetSafeReturnUrl()
    {
        if (string.IsNullOrWhiteSpace(ReturnUrl) ||
            !ReturnUrl.StartsWith(
                "/",
                StringComparison.Ordinal) ||
            ReturnUrl.StartsWith(
                "//",
                StringComparison.Ordinal))
        {
            return "/";
        }

        return ReturnUrl;
    }
}
