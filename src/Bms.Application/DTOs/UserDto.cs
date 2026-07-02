using System;
using System.ComponentModel.DataAnnotations;
using Bms.Domain.Enums;

namespace Bms.Application.DTOs;

public class UserDto
{
    public Guid Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public UserRole Role { get; set; }
    public Guid? DepartmentId { get; set; }
    public string DepartmentName { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }

    public string FullName => $"{FirstName} {LastName}".Trim();
}

public class CreateUserDto
{
    [Required(ErrorMessage = "Username is required.")]
    [StringLength(50, MinimumLength = 3, ErrorMessage = "Username must be between 3 and 50 characters.")]
    [RegularExpression(@"^[a-zA-Z0-9_\-\.]+$", ErrorMessage = "Username can only contain alphanumeric characters, underscores, hyphens, or periods.")]
    public string Username { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email address is required.")]
    [EmailAddress(ErrorMessage = "Invalid email address format.")]
    [StringLength(100, ErrorMessage = "Email cannot exceed 100 characters.")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password is required.")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters long.")]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "First Name is required.")]
    [StringLength(50, ErrorMessage = "First Name cannot exceed 50 characters.")]
    public string FirstName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Last Name is required.")]
    [StringLength(50, ErrorMessage = "Last Name cannot exceed 50 characters.")]
    public string LastName { get; set; } = string.Empty;

    [Required(ErrorMessage = "System role is required.")]
    public UserRole Role { get; set; }

    public Guid? DepartmentId { get; set; }
}

public class UpdateUserDto
{
    [Required(ErrorMessage = "User ID is required.")]
    public Guid Id { get; set; }

    [Required(ErrorMessage = "Email address is required.")]
    [EmailAddress(ErrorMessage = "Invalid email address format.")]
    [StringLength(100, ErrorMessage = "Email cannot exceed 100 characters.")]
    public string Email { get; set; } = string.Empty;

    [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters long.")]
    public string? Password { get; set; } // Optional password update

    [Required(ErrorMessage = "First Name is required.")]
    [StringLength(50, ErrorMessage = "First Name cannot exceed 50 characters.")]
    public string FirstName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Last Name is required.")]
    [StringLength(50, ErrorMessage = "Last Name cannot exceed 50 characters.")]
    public string LastName { get; set; } = string.Empty;

    [Required(ErrorMessage = "System role is required.")]
    public UserRole Role { get; set; }

    public Guid? DepartmentId { get; set; }

    [Required(ErrorMessage = "Account status is required.")]
    public bool IsActive { get; set; }
}
