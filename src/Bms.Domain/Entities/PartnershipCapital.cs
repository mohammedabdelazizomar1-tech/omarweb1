using System;

namespace Bms.Domain.Entities;

public class PartnershipCapital : BaseEntity
{
    public Guid TenantId { get; set; }
    public string ShareholderName { get; set; } = string.Empty; // e.g. علاء, خالد, عمر, وحيد, كيلاني
    public decimal AmountSar { get; set; } // Amount contributed in Saudi Riyal
    public decimal AmountEgp { get; set; } // Amount contributed in Egyptian Pound
    public decimal HistoricalRate { get; set; } = 13.0m; // SAR exchange rate during contribution
    
    // Profit distribution configurations
    public decimal ShareRatio { get; set; } // Shareholding fraction (المساهمة)
    public decimal ProfitShareRatio { get; set; } // Profit percentage (نسبة الربح)
    
    public string Notes { get; set; } = string.Empty;
}
