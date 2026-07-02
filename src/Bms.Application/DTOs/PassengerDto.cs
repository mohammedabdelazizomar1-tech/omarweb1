using System;
using System.ComponentModel.DataAnnotations;

namespace Bms.Application.DTOs;

public class PassengerDto
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string PassportNumber { get; set; } = string.Empty;
    public string Nationality { get; set; } = "Egyptian";
    public DateOnly? DateOfBirth { get; set; }
    public string PhoneNumber { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class CreatePassengerDto
{
    [Required(ErrorMessage = "Full Name is required.")]
    [StringLength(100, ErrorMessage = "Full Name cannot exceed 100 characters.")]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Passport Number is required.")]
    [StringLength(20, ErrorMessage = "Passport Number cannot exceed 20 characters.")]
    [RegularExpression(@"^[A-Z0-9]+$", ErrorMessage = "Passport Number must contain alphanumeric characters only.")]
    public string PassportNumber { get; set; } = string.Empty;

    [Required(ErrorMessage = "Nationality is required.")]
    [StringLength(50, ErrorMessage = "Nationality cannot exceed 50 characters.")]
    public string Nationality { get; set; } = "Egyptian";

    public DateOnly? DateOfBirth { get; set; }

    [Phone(ErrorMessage = "Invalid phone number.")]
    public string PhoneNumber { get; set; } = string.Empty;
}
