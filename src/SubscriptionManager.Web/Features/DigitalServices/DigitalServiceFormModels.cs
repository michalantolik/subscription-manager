using System.ComponentModel.DataAnnotations;

namespace SubscriptionManager.Web.Features.DigitalServices;

/// <summary>
/// Form data for creating a digital service.
/// </summary>
public sealed class CreateDigitalServiceFormModel
{
    [Required]
    [StringLength(200)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [StringLength(200)]
    public string Category { get; set; } = string.Empty;

    [Url]
    [StringLength(500)]
    public string? ManagementUrl { get; set; }
}
