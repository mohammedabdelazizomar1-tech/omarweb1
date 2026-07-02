using System;
using System.ComponentModel.DataAnnotations;
using Bms.Domain.Enums;

namespace Bms.Application.DTOs;

public class GatewayPortalDto
{
    public Guid Id { get; set; }
    public DateOnly TransactionDate { get; set; }
    public decimal Amount { get; set; }
    public int Count { get; set; }
    public string ServiceType { get; set; } = string.Empty;
    public Guid? PartnerId { get; set; }
    public string PartnerName { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class CreateGatewayPortalDto
{
    [Required(ErrorMessage = "Transaction Date is required.")]
    public DateOnly TransactionDate { get; set; } = DateOnly.FromDateTime(DateTime.UtcNow);

    [Required(ErrorMessage = "Purchase Amount is required.")]
    [Range(0, 10000000, ErrorMessage = "Amount must be a non-negative number.")]
    public decimal Amount { get; set; }

    [Required(ErrorMessage = "Slot Count is required.")]
    [Range(1, 10000, ErrorMessage = "Slot Count must be at least 1.")]
    public int Count { get; set; }

    [Required(ErrorMessage = "Portal Service Type is required.")]
    public PortalServiceType ServiceType { get; set; }

    public Guid? PartnerId { get; set; }

    [StringLength(250, ErrorMessage = "Notes cannot exceed 250 characters.")]
    public string Notes { get; set; } = string.Empty;
}
