using SubscriptionManager.Blazor.Features.Authentication;
using System.Globalization;

namespace SubscriptionManager.Blazor.Components.Pages;

public partial class ForgotPassword
{
    private readonly ForgotPasswordFormModel _model = new();

    private bool _isSubmitting;
    private bool _isCompleted;
    private bool _hasError;

    private async Task SubmitAsync()
    {
        if (_isSubmitting)
        {
            return;
        }

        _isSubmitting = true;
        _hasError = false;

        try
        {
            _model.Email = _model.Email.Trim();

            var result = await ApiClient.ForgotPasswordAsync(
                _model.Email,
                CultureInfo.CurrentUICulture.TwoLetterISOLanguageName);

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
