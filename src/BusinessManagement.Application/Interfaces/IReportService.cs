using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BusinessManagement.Application.DTOs;

namespace BusinessManagement.Application.Interfaces;

public interface IReportService
{
    Task<CustomerReportDto> GetCustomerReportAsync(DateTime? fromDate, DateTime? toDate);
    Task<BookingReportDto> GetBookingReportAsync(DateTime? fromDate, DateTime? toDate);
    Task<VisaReportDto> GetVisaReportAsync(DateTime? fromDate, DateTime? toDate);
    Task<CashReportDto> GetCashReportAsync(DateTime? fromDate, DateTime? toDate);
    Task<ExpenseReportDto> GetExpenseReportAsync(DateTime? fromDate, DateTime? toDate);
    Task<RevenueReportDto> GetRevenueReportAsync(DateTime? fromDate, DateTime? toDate);
    Task<ProfitReportDto> GetProfitReportAsync(DateTime? fromDate, DateTime? toDate);
    Task<IEnumerable<PartnerReportDto>> GetPartnerReportAsync();
    Task<IEnumerable<PeriodicReportDto>> GetPeriodicReportsAsync(string rangeType, int year);
}
