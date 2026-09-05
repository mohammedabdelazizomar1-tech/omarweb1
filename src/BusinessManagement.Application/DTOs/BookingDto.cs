using System;
using System.ComponentModel.DataAnnotations;

namespace BusinessManagement.Application.DTOs;

public class BookingDto
{
    public Guid Id { get; set; }
    public string BookingNumber { get; set; } = string.Empty;
    public Guid PassengerId { get; set; }
    public string PassengerName { get; set; } = string.Empty;
    public string PassengerPassport { get; set; } = string.Empty;
    public Guid PartnerId { get; set; }
    public string PartnerName { get; set; } = string.Empty;
    public DateOnly TravelDate { get; set; }

    // Pricing cost breakdowns
    public decimal NetCost { get; set; }
    public decimal BarcodeCost { get; set; }
    public decimal CompanyCost { get; set; }
    public decimal AirportCost { get; set; }
    public decimal ProgramCost { get; set; }
    public decimal TicketCost { get; set; }
    public decimal BusCost { get; set; }

    // Totals
    public decimal TotalCost { get; set; }
    public decimal SellingPrice { get; set; }
    public decimal NetProfit { get; set; }
    public decimal PaymentsCollected { get; set; }
    public decimal RemainingBalance { get; set; }

    public bool HasQrCode { get; set; }
    public string Notes { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public string? GroupNumber { get; set; }
}

public class CreateBookingDto
{
    public Guid? PassengerId { get; set; }
    public List<Guid>? PassengerIds { get; set; }

    [Required(ErrorMessage = "Attributed Partner is required.")]
    public Guid PartnerId { get; set; }

    [Required(ErrorMessage = "Travel Date is required.")]
    public DateOnly TravelDate { get; set; }

    [Range(0, 1000000, ErrorMessage = "Net Cost must be positive.")]
    public decimal? NetCost { get; set; }

    [Range(0, 1000000, ErrorMessage = "Barcode Cost must be positive.")]
    public decimal? BarcodeCost { get; set; }

    [Range(0, 1000000, ErrorMessage = "Company Cost must be positive.")]
    public decimal? CompanyCost { get; set; }

    [Range(0, 1000000, ErrorMessage = "Airport Cost must be positive.")]
    public decimal? AirportCost { get; set; }

    [Range(0, 1000000, ErrorMessage = "Program Cost must be positive.")]
    public decimal? ProgramCost { get; set; }

    [Range(0, 1000000, ErrorMessage = "Ticket Cost must be positive.")]
    public decimal? TicketCost { get; set; }

    [Range(0, 1000000, ErrorMessage = "Bus Cost must be positive.")]
    public decimal? BusCost { get; set; }

    [Range(0, 1000000, ErrorMessage = "Selling Price must be positive.")]
    public decimal? SellingPrice { get; set; }

    [Range(0, 1000000, ErrorMessage = "Payments Collected must be positive.")]
    public decimal? PaymentsCollected { get; set; }

    public string? PaymentMethod { get; set; }
    public string? ReceiptNumber { get; set; }
    public DateOnly? PaymentDate { get; set; }

    public Dictionary<Guid, decimal?>? PassengerPayments { get; set; }
    public Dictionary<Guid, string?>? PassengerPaymentMethods { get; set; }
    public Dictionary<Guid, string?>? PassengerReceiptNumbers { get; set; }
    public Dictionary<Guid, DateOnly?>? PassengerPaymentDates { get; set; }

    public string? GroupNumber { get; set; }

    public bool HasQrCode { get; set; }
    public bool RegisterAsGroup { get; set; } = false;
    public string Notes { get; set; } = string.Empty;
}

public class ExcelImportResultDto
{
    public int CapitalImportedCount { get; set; }
    public int BookingsImportedCount { get; set; }
    public int ExternalVisasImportedCount { get; set; }
    public int SafeTransactionsImportedCount { get; set; }
    public List<string> Messages { get; set; } = new List<string>();
}





