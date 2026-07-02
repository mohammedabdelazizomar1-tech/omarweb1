using System;
using System.ComponentModel.DataAnnotations;

namespace Bms.Application.DTOs;

public class ExternalVisaDto
{
    public Guid Id { get; set; }
    public string PassengerName { get; set; } = string.Empty;
    public string Affiliation { get; set; } = string.Empty;
    
    public decimal NetCost { get; set; }
    public decimal BarcodeCost { get; set; }
    public decimal AgentCommission { get; set; }
    public decimal AgreementCost { get; set; }
    public decimal TicketCost { get; set; }
    public decimal AirportCost { get; set; }
    public decimal BusCost { get; set; }

    public decimal TotalCost { get; set; }
    public decimal SellingPrice { get; set; }
    public decimal NetProfit { get; set; }
    public decimal AmountPaid { get; set; }
    public decimal RemainingBalance { get; set; }

    public string Notes { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class CreateExternalVisaDto
{
    [Required(ErrorMessage = "Passenger Name is required.")]
    [StringLength(100, ErrorMessage = "Passenger Name cannot exceed 100 characters.")]
    public string PassengerName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Affiliation is required.")]
    [StringLength(100, ErrorMessage = "Affiliation cannot exceed 100 characters.")]
    public string Affiliation { get; set; } = string.Empty;

    [Range(0, 1000000, ErrorMessage = "Net Cost must be positive.")]
    public decimal NetCost { get; set; }

    [Range(0, 1000000, ErrorMessage = "Barcode Cost must be positive.")]
    public decimal BarcodeCost { get; set; }

    [Range(0, 1000000, ErrorMessage = "Agent Commission must be positive.")]
    public decimal AgentCommission { get; set; }

    [Range(0, 1000000, ErrorMessage = "Agreement Cost must be positive.")]
    public decimal AgreementCost { get; set; }

    [Range(0, 1000000, ErrorMessage = "Ticket Cost must be positive.")]
    public decimal TicketCost { get; set; }

    [Range(0, 1000000, ErrorMessage = "Airport Cost must be positive.")]
    public decimal AirportCost { get; set; }

    [Range(0, 1000000, ErrorMessage = "Bus Cost must be positive.")]
    public decimal BusCost { get; set; }

    [Range(0, 1000000, ErrorMessage = "Selling Price must be positive.")]
    public decimal SellingPrice { get; set; }

    [Range(0, 1000000, ErrorMessage = "Amount Paid must be positive.")]
    public decimal AmountPaid { get; set; }

    public string Notes { get; set; } = string.Empty;
}
