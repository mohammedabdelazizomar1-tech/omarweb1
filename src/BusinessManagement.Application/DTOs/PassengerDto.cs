using System;
using System.ComponentModel.DataAnnotations;

namespace BusinessManagement.Application.DTOs;

public class PassengerDto
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string PassportNumber { get; set; } = string.Empty;
    public string Nationality { get; set; } = "Egyptian";
    public DateOnly? DateOfBirth { get; set; }
    public string PhoneNumber { get; set; } = string.Empty;
    public Guid? PartnerId { get; set; }
    public string? GroupNumber { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreatePassengerDto
{
    [Required(ErrorMessage = "Full Name is required.")]
    [StringLength(100, ErrorMessage = "Full Name cannot exceed 100 characters.")]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Passport Number is required.")]
    [StringLength(20, ErrorMessage = "Passport Number cannot exceed 20 characters.")]
    [RegularExpression(@"^[a-zA-Z0-9]+$", ErrorMessage = "Passport Number must contain alphanumeric characters only.")]
    public string PassportNumber { get; set; } = string.Empty;

    public DateOnly? PassportExpiry { get; set; }

    [StringLength(14, MinimumLength = 14, ErrorMessage = "National ID must be exactly 14 characters.")]
    public string? NationalId { get; set; }

    public DateOnly? DateOfBirth { get; set; }

    [StringLength(50, ErrorMessage = "Nationality cannot exceed 50 characters.")]
    public string? Nationality { get; set; } = "Egyptian";

    public string? Gender { get; set; }

    [Phone(ErrorMessage = "Invalid phone number.")]
    public string? PhoneNumber { get; set; }

    [EmailAddress(ErrorMessage = "Invalid email address format.")]
    public string? Email { get; set; }

    public string? Address { get; set; }

    public string? EmergencyContactName { get; set; }

    public string? EmergencyContactPhone { get; set; }

    public Guid? PartnerId { get; set; }
}

public class PassengerImportResultDto
{
    public int TotalRows { get; set; }
    public int ImportedCount { get; set; }
    public int DuplicateCount { get; set; }
    public List<PassengerImportErrorDto> ImportErrors { get; set; } = new List<PassengerImportErrorDto>();
}

public class PassengerImportErrorDto
{
    public string Message { get; set; } = string.Empty;
}





