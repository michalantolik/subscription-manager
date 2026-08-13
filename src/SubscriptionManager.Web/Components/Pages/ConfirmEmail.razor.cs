using Microsoft.AspNetCore.Components;

namespace SubscriptionManager.Web.Components.Pages;

public partial class ConfirmEmail
{
    [Parameter]
    [SupplyParameterFromQuery(Name = "userId")]
    public Guid? UserId { get; set; }

    [Parameter]
    [SupplyParameterFromQuery(Name = "token")]
    public string? ConfirmationToken { get; set; }

    private bool _isConfirming = true;
    private bool _isConfirmed;

    protected override async Task OnParametersSetAsync()
    {
        if (!UserId.HasValue ||
            UserId.Value == Guid.Empty ||
            string.IsNullOrWhiteSpace(ConfirmationToken))
        {
            _isConfirming = false;
            return;
        }

        try
        {
            var result = await ApiClient.ConfirmEmailAsync(
                UserId.Value,
                ConfirmationToken);

            _isConfirmed = result.Succeeded;
        }
        catch (HttpRequestException)
        {
            _isConfirmed = false;
        }
        finally
        {
            _isConfirming = false;
        }
    }
}
