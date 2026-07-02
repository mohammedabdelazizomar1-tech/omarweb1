using System;
using System.Collections.Generic;

namespace Bms.Application.DTOs;

public class TravelDashboardDto
{
    // Safe balances
    public decimal SafeBalanceEgp { get; set; }
    public decimal SafeBalanceSar { get; set; }

    // Quotas and counts per partner
    public List<PartnerQuotaMetric> PartnerQuotas { get; set; } = new();

    // Financial KPIs
    public decimal TotalSalesInvoiced { get; set; }
    public decimal TotalPaymentsCollected { get; set; }
    public decimal TotalRemainingReceivable { get; set; }
    public decimal TotalAccumulatedProfit { get; set; }
    
    // Portal slots status
    public int AvailableQrCodes { get; set; }
    public int AvailableVipSlots { get; set; }

    public List<RecentBookingSummary> RecentBookings { get; set; } = new();
}

public class PartnerQuotaMetric
{
    public Guid PartnerId { get; set; }
    public string PartnerName { get; set; } = string.Empty;
    public int BookingsCount { get; set; }
    public int QrCount { get; set; }
    public int NonQrCount { get; set; }
    public int QuotaLimit { get; set; }
    public decimal TotalProfit { get; set; }
    public decimal CollectedPayments { get; set; }
    public decimal RemainingBalance { get; set; }
}

public class RecentBookingSummary
{
    public Guid BookingId { get; set; }
    public string BookingNumber { get; set; } = string.Empty;
    public string PassengerName { get; set; } = string.Empty;
    public string PartnerName { get; set; } = string.Empty;
    public DateOnly TravelDate { get; set; }
    public decimal SellingPrice { get; set; }
    public string Status { get; set; } = string.Empty;
    public bool HasQrCode { get; set; }
}
