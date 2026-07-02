using System.ComponentModel.DataAnnotations;

namespace Bms.Application.DTOs;

public class LoginRequestDto
{
    [Required(ErrorMessage = "Username or Email is required.")]
    public string UsernameOrEmail { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password is required.")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    public bool RememberMe { get; set; } = false;
}
