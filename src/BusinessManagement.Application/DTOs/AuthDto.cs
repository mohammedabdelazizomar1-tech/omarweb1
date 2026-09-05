using System.ComponentModel.DataAnnotations;

namespace BusinessManagement.Application.DTOs;

public class LoginRequestDto
{
    [Required(ErrorMessage = "Username or Email is required.")]
    public string UsernameOrEmail { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password is required.")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    public bool RememberMe { get; set; } = false;
}

public class ForgotPasswordDto
{
    [Required(ErrorMessage = "البريد الإلكتروني مطلوب.")]
    [EmailAddress(ErrorMessage = "صيغة البريد الإلكتروني غير صحيحة.")]
    public string Email { get; set; } = string.Empty;
}

public class ResetPasswordDto
{
    [Required(ErrorMessage = "البريد الإلكتروني مطلوب.")]
    [EmailAddress(ErrorMessage = "صيغة البريد الإلكتروني غير صحيحة.")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "كود استعادة كلمة المرور مطلوب.")]
    public string Token { get; set; } = string.Empty;

    [Required(ErrorMessage = "كلمة المرور الجديدة مطلوبة.")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "يجب أن تتكون كلمة المرور من 6 أحرف على الأقل.")]
    [DataType(DataType.Password)]
    public string NewPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "تأكيد كلمة المرور مطلوب.")]
    [Compare("NewPassword", ErrorMessage = "كلمات المرور غير متطابقة.")]
    [DataType(DataType.Password)]
    public string ConfirmPassword { get; set; } = string.Empty;
}





