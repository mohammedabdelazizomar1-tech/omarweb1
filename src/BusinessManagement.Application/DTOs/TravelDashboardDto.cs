using System;
using System.Collections.Generic;

namespace BusinessManagement.Application.DTOs;

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
    public decimal TotalExpenses { get; set; }

    // Counts
    public int PendingPaymentsCount { get; set; }
    public int CompletedPaymentsCount { get; set; }
    public int UpcomingTripsCount { get; set; }
    public int TotalPassengersCount { get; set; }
    public int TotalBookingsCount { get; set; }
    public int TotalExternalVisasCount { get; set; }
    public int ActiveGatewaysCount { get; set; }

    // Portal slots status
    public int AvailableQrCodes { get; set; }
    public int AvailableVipSlots { get; set; }

    // Data Feeds
    public List<RecentBookingSummary> RecentBookings { get; set; } = new();
    public List<ActivityLogDto> RecentActivities { get; set; } = new();

    // Chart Datasets
    public List<string> MonthlyRevenueLabels { get; set; } = new();
    public List<decimal> MonthlyRevenueData { get; set; } = new();
    public List<decimal> MonthlyProfitData { get; set; } = new();

    public List<string> CashFlowLabels { get; set; } = new();
    public List<decimal> CashFlowDeposits { get; set; } = new();
    public List<decimal> CashFlowWithdrawals { get; set; } = new();

    // Expense breakdowns
    public decimal GatewayExpenses { get; set; }
    public decimal AgencyExpenses { get; set; }
    public decimal AviationExpenses { get; set; }
    public decimal OtherExpenses { get; set; }

    // Barcode linking
    public decimal GatewayBarcodeSpent { get; set; }
    public decimal BookingsBarcodeAllocated { get; set; }

    // Partner Ledger
    public List<PartnerLedgerMetric> PartnerLedgers { get; set; } = new();
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
    public decimal ProfitShare { get; set; }
    public decimal Withdrawals { get; set; }
}

public class PartnerLedgerMetric
{
    public Guid PartnerId { get; set; }
    public string PartnerName { get; set; } = string.Empty;
    public int ClientsCount { get; set; }
    public int QrCount { get; set; }
    public int NonQrCount { get; set; }
    public decimal TotalAccount { get; set; }
    public decimal PaidAmount { get; set; }
    public decimal RemainingBalance { get; set; }
    public decimal BookingsProfit { get; set; }
    public decimal AverageProfitPerClient { get; set; }
    public decimal ProfitShare { get; set; }
    public decimal Withdrawals { get; set; }
    public decimal NetLedgerBalance { get; set; }
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

public class ActivityLogDto
{
    public string UserId { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string IpAddress { get; set; } = string.Empty;
    public string UserAgent { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
