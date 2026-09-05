using System;
using System.Collections.Generic;

namespace BusinessManagement.Application.DTOs;

public class ReportDto
{
    // Common filters placeholder
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
}

public class CustomerReportDto
{
    public int TotalCustomers { get; set; }
    public int TotalGroups { get; set; }
    public List<KeyValuePair<string, int>> CustomersByNationality { get; set; } = new();
    public List<KeyValuePair<string, int>> CustomersByGender { get; set; } = new();
    public List<CustomerFrequencyDto> MostFrequentCustomers { get; set; } = new();
}

public class CustomerFrequencyDto
{
    public string Name { get; set; } = string.Empty;
    public string PassportNumber { get; set; } = string.Empty;
    public int BookingsCount { get; set; }
}

public class BookingReportDto
{
    public int TotalBookingsCount { get; set; }
    public decimal TotalSellingPrice { get; set; }
    public decimal TotalCost { get; set; }
    public decimal TotalNetProfit { get; set; }
    public List<KeyValuePair<string, int>> BookingsByStatus { get; set; } = new();
    public List<BookingDetailReportDto> DetailedBookings { get; set; } = new();
}

public class BookingDetailReportDto
{
    public Guid BookingId { get; set; }
    public string PassengerName { get; set; } = string.Empty;
    public string PartnerName { get; set; } = string.Empty;
    public DateOnly TravelDate { get; set; }
    public decimal SellingPrice { get; set; }
    public decimal NetCost { get; set; }
    public decimal Profit { get; set; }
}

public class VisaReportDto
{
    public int TotalVisas { get; set; }
    public int QrCodeVisasCount { get; set; }
    public int VipSlotsCount { get; set; }
    public int ExternalVisasCount { get; set; }
    public List<VisaDetailDto> VisasList { get; set; } = new();
}

public class VisaDetailDto
{
    public string PassengerName { get; set; } = string.Empty;
    public string PassportNumber { get; set; } = string.Empty;
    public string VisaType { get; set; } = string.Empty; // e.g. B2C, VIP, External
    public bool HasQrCode { get; set; }
    public string Status { get; set; } = string.Empty;
}

public class CashReportDto
{
    public decimal CurrentSafeEgpBalance { get; set; }
    public decimal CurrentSafeSarBalance { get; set; }
    public decimal TotalEgpDeposits { get; set; }
    public decimal TotalEgpWithdrawals { get; set; }
    public decimal TotalSarDeposits { get; set; }
    public decimal TotalSarWithdrawals { get; set; }
    public List<CashTransactionDetailDto> TransactionsList { get; set; } = new();
}

public class CashTransactionDetailDto
{
    public DateTime Date { get; set; }
    public string ReferenceNumber { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty; // Deposit, Withdrawal, Transfer
    public decimal Amount { get; set; }
    public string Currency { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}

public class ExpenseReportDto
{
    public decimal TotalExpenses { get; set; }
    public List<CategorySummaryDto> ExpensesByCategory { get; set; } = new();
}

public class CategorySummaryDto
{
    public string AccountCode { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public double Percentage { get; set; }
}

public class RevenueReportDto
{
    public decimal TotalRevenues { get; set; }
    public List<CategorySummaryDto> RevenuesByCategory { get; set; } = new();
}

public class ProfitReportDto
{
    public decimal TotalRevenues { get; set; }
    public decimal TotalExpenses { get; set; }
    public decimal NetProfit { get; set; }
    public double ProfitMarginPercentage { get; set; }
    public List<MonthlyProfitDetailDto> ProfitTimeline { get; set; } = new();
}

public class MonthlyProfitDetailDto
{
    public string MonthName { get; set; } = string.Empty;
    public decimal Revenue { get; set; }
    public decimal Expense { get; set; }
    public decimal Profit { get; set; }
}

public class PartnerReportDto
{
    public string ShareholderName { get; set; } = string.Empty;
    public decimal AmountSar { get; set; }
    public decimal AmountEgp { get; set; }
    public decimal ShareRatio { get; set; }
    public decimal ProfitShareRatio { get; set; }
    public decimal EstimatedProfitQuota { get; set; } // Based on total retained profit
}

public class PeriodicReportDto
{
    public string PeriodType { get; set; } = string.Empty; // Daily, Monthly, Annual
    public string PeriodLabel { get; set; } = string.Empty;
    public int BookingsCount { get; set; }
    public decimal Revenues { get; set; }
    public decimal Expenses { get; set; }
    public decimal NetProfit { get; set; }
}
