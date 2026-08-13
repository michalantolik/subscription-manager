using Microsoft.AspNetCore.Components;
using SubscriptionManager.Web.Features.Authentication;

namespace SubscriptionManager.Web.Components.Pages;

public partial class ResetPassword
{
    [Parameter]
    [SupplyParameterFromQuery(Name = "userId")]
    public Guid? UserId { get; set; }

    [Parameter]
    [SupplyParameterFromQuery(Name = "token")]
    public string? ResetToken { get; set; }

    private readonly ResetPasswordFormModel _model = new();

    private bool _isSubmitting;
    private bool _isCompleted;
    private bool _hasError;

    private bool _hasValidResetLink =>
        UserId.HasValue &&
        UserId.Value != Guid.Empty &&
        !string.IsNullOrWhiteSpace(ResetToken);

    private async Task SubmitAsync()
    {
        if (_isSubmitting || !_hasValidResetLink)
        {
            return;
        }

        _isSubmitting = true;
        _hasError = false;

        try
        {
            var result = await ApiClient.ResetPasswordAsync(
                UserId!.Value,
                ResetToken!,
                _model.NewPassword);

            if (result.Succeeded)
            {
                _isCompleted = true;
            }
            else
            {
                _hasError = true;
            }
        }
        catch (Exception)
        {
            _hasError = true;
        }
        finally
        {
            _isSubmitting = false;
        }
    }
}
