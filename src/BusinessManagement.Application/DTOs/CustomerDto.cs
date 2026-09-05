using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BusinessManagement.Application.DTOs;

public class CustomerListDto
{
    public IEnumerable<CustomerDetailsDto> Customers { get; set; } = new List<CustomerDetailsDto>();
    public int TotalCount { get; set; }
    public int PageIndex { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
    public bool HasPreviousPage => PageIndex > 1;
    public bool HasNextPage => PageIndex < TotalPages;
}

public class CustomerDetailsDto
{
    public Guid Id { get; set; }

    [Required(ErrorMessage = "الاسم بالكامل مطلوب.")]
    [StringLength(100, ErrorMessage = "الاسم لا يمكن أن يزيد عن 100 حرف.")]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "رقم جواز السفر مطلوب.")]
    [StringLength(20, ErrorMessage = "رقم جواز السفر لا يمكن أن يزيد عن 20 حرف.")]
    public string PassportNumber { get; set; } = string.Empty;

    public DateOnly? PassportExpiry { get; set; }

    [StringLength(14, MinimumLength = 14, ErrorMessage = "الرقم القومي يجب أن يتكون من 14 رقماً.")]
    public string? NationalId 
    { 
        get => _nationalId; 
        set => _nationalId = string.IsNullOrWhiteSpace(value) ? null : value; 
    }
    private string? _nationalId;

    public DateOnly? DateOfBirth { get; set; }

    public string Nationality { get; set; } = "Egyptian";

    public string? Gender { get; set; } // Male, Female

    public string? PhoneNumber { get; set; }

    [EmailAddress(ErrorMessage = "البريد الإلكتروني غير صحيح.")]
    public string? Email { get; set; }

    public string? Address { get; set; }

    public string? EmergencyContactName { get; set; }
    public string? EmergencyContactPhone { get; set; }

    public Guid? PartnerId { get; set; }
    public string? PartnerName { get; set; }
    public string? GroupNumber { get; set; }

    public DateTime CreatedAt { get; set; }
}

public class PassengerNoteDto
{
    public Guid Id { get; set; }
    public Guid PassengerId { get; set; }
    public string NoteText { get; set; } = string.Empty;
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class AttachmentDto
{
    public Guid Id { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long FileSizeBytes { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CustomerBookingHistoryDto
{
    public Guid BookingId { get; set; }
    public string BookingNumber { get; set; } = string.Empty;
    public string PartnerName { get; set; } = string.Empty;
    public string QrStatus { get; set; } = string.Empty;
    public DateOnly TravelDate { get; set; }
    public decimal SellingPrice { get; set; }
    public decimal AmountPaid { get; set; }
    public decimal RemainingBalance { get; set; }
    public string Status { get; set; } = string.Empty;
}

public class CustomerImportResultDto
{
    public int TotalRows { get; set; }
    public int ImportedCount { get; set; }
    public int DuplicateCount { get; set; }
    public List<CustomerImportErrorDto> ImportErrors { get; set; } = new();
}

public class CustomerImportErrorDto
{
    public int RowIndex { get; set; }
    public string Message { get; set; } = string.Empty;
}
