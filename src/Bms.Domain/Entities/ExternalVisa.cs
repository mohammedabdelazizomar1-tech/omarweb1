using System;

namespace Bms.Domain.Entities;

public class ExternalVisa : BaseEntity
{
    public Guid TenantId { get; set; }
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
}
