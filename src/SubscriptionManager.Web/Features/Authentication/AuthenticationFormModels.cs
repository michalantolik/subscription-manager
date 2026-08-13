using System.ComponentModel.DataAnnotations;

namespace SubscriptionManager.Web.Features.Authentication;

/// <summary>
/// Form data for user registration.
/// </summary>
public sealed class RegisterFormModel
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MinLength(8)]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    [Required]
    [Compare(nameof(Password))]
    [DataType(DataType.Password)]
    public string ConfirmPassword { get; set; } = string.Empty;
}

/// <summary>
/// Form data for user login.
/// </summary>
public sealed class LoginFormModel
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;
}

/// <summary>
/// Form data for password recovery.
/// </summary>
public sealed class ForgotPasswordFormModel
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;
}

/// <summary>
/// Form data for password reset.
/// </summary>
public sealed class ResetPasswordFormModel
{
    [Required]
    public Guid UserId { get; set; }

    [Required]
    public string ResetToken { get; set; } = string.Empty;

    [Required]
    [MinLength(8)]
    [DataType(DataType.Password)]
    public string NewPassword { get; set; } = string.Empty;

    [Required]
    [Compare(nameof(NewPassword))]
    [DataType(DataType.Password)]
    public string ConfirmPassword { get; set; } = string.Empty;
}
