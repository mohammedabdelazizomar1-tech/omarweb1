using System;
using System.ComponentModel.DataAnnotations;

namespace Bms.Application.DTOs;

public class PartnershipCapitalDto
{
    public Guid Id { get; set; }
    public string ShareholderName { get; set; } = string.Empty;
    public decimal AmountSar { get; set; }
    public decimal AmountEgp { get; set; }
    public decimal HistoricalRate { get; set; }
    public decimal ShareRatio { get; set; }
    public decimal ProfitShareRatio { get; set; }
    public string Notes { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class CreatePartnershipCapitalDto
{
    [Required(ErrorMessage = "Shareholder Name is required.")]
    [StringLength(100, ErrorMessage = "Shareholder Name cannot exceed 100 characters.")]
    public string ShareholderName { get; set; } = string.Empty;

    [Required(ErrorMessage = "SAR Amount is required.")]
    [Range(0, 100000000, ErrorMessage = "SAR Amount must be non-negative.")]
    public decimal AmountSar { get; set; }

    [Required(ErrorMessage = "EGP Amount is required.")]
    [Range(0, 100000000, ErrorMessage = "EGP Amount must be non-negative.")]
    public decimal AmountEgp { get; set; }

    [Required(ErrorMessage = "Historical Exchange Rate is required.")]
    [Range(0.0001, 1000, ErrorMessage = "Historical exchange rate must be positive.")]
    public decimal HistoricalRate { get; set; } = 13.0m;

    [Required(ErrorMessage = "Shareholding Ratio (0 to 1) is required.")]
    [Range(0, 1, ErrorMessage = "Shareholding ratio must be between 0 and 1.")]
    public decimal ShareRatio { get; set; }

    [Required(ErrorMessage = "Profit Share Ratio (0 to 1) is required.")]
    [Range(0, 1, ErrorMessage = "Profit share ratio must be between 0 and 1.")]
    public decimal ProfitShareRatio { get; set; }

    [StringLength(250, ErrorMessage = "Notes cannot exceed 250 characters.")]
    public string Notes { get; set; } = string.Empty;
}
