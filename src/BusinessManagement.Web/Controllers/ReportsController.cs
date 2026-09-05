using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using BusinessManagement.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace BusinessManagement.Web.Controllers;

[Authorize]
public class ReportsController : Controller
{
    private readonly IReportService _reportService;
    private readonly ILogger<ReportsController> _logger;

    public ReportsController(IReportService reportService, ILogger<ReportsController> logger)
    {
        _reportService = reportService;
        _logger = logger;
    }

    [HttpGet]
    public IActionResult Index()
    {
        ViewData["PageHeader"] = "مركز التقارير الشامل";
        return View();
    }

    [HttpGet]
    public async Task<IActionResult> CustomerReport(DateTime? fromDate, DateTime? toDate)
    {
        ViewBag.FromDate = fromDate?.ToString("yyyy-MM-dd");
        ViewBag.ToDate = toDate?.ToString("yyyy-MM-dd");
        var report = await _reportService.GetCustomerReportAsync(fromDate, toDate);
        return View(report);
    }

    [HttpGet]
    public async Task<IActionResult> BookingReport(DateTime? fromDate, DateTime? toDate)
    {
        ViewBag.FromDate = fromDate?.ToString("yyyy-MM-dd");
        ViewBag.ToDate = toDate?.ToString("yyyy-MM-dd");
        var report = await _reportService.GetBookingReportAsync(fromDate, toDate);
        return View(report);
    }

    [HttpGet]
    public async Task<IActionResult> VisaReport(DateTime? fromDate, DateTime? toDate)
    {
        ViewBag.FromDate = fromDate?.ToString("yyyy-MM-dd");
        ViewBag.ToDate = toDate?.ToString("yyyy-MM-dd");
        var report = await _reportService.GetVisaReportAsync(fromDate, toDate);
        return View(report);
    }

    [HttpGet]
    public async Task<IActionResult> CashReport(DateTime? fromDate, DateTime? toDate)
    {
        ViewBag.FromDate = fromDate?.ToString("yyyy-MM-dd");
        ViewBag.ToDate = toDate?.ToString("yyyy-MM-dd");
        var report = await _reportService.GetCashReportAsync(fromDate, toDate);
        return View(report);
    }

    [HttpGet]
    public async Task<IActionResult> ExpenseReport(DateTime? fromDate, DateTime? toDate)
    {
        ViewBag.FromDate = fromDate?.ToString("yyyy-MM-dd");
        ViewBag.ToDate = toDate?.ToString("yyyy-MM-dd");
        var report = await _reportService.GetExpenseReportAsync(fromDate, toDate);
        return View(report);
    }

    [HttpGet]
    public async Task<IActionResult> RevenueReport(DateTime? fromDate, DateTime? toDate)
    {
        ViewBag.FromDate = fromDate?.ToString("yyyy-MM-dd");
        ViewBag.ToDate = toDate?.ToString("yyyy-MM-dd");
        var report = await _reportService.GetRevenueReportAsync(fromDate, toDate);
        return View(report);
    }

    [HttpGet]
    public async Task<IActionResult> ProfitReport(DateTime? fromDate, DateTime? toDate)
    {
        ViewBag.FromDate = fromDate?.ToString("yyyy-MM-dd");
        ViewBag.ToDate = toDate?.ToString("yyyy-MM-dd");
        var report = await _reportService.GetProfitReportAsync(fromDate, toDate);
        return View(report);
    }

    [HttpGet]
    public async Task<IActionResult> PartnerReport()
    {
        var report = await _reportService.GetPartnerReportAsync();
        return View(report);
    }

    [HttpGet]
    public async Task<IActionResult> PeriodicReport(string rangeType = "daily", int? year = null)
    {
        int targetYear = year ?? DateTime.Today.Year;
        ViewBag.RangeType = rangeType;
        ViewBag.Year = targetYear;
        var report = await _reportService.GetPeriodicReportsAsync(rangeType, targetYear);
        return View(report);
    }

    // Excel Exports (.xlsx)
    [HttpGet]
    public async Task<IActionResult> ExportCustomerExcel(DateTime? fromDate, DateTime? toDate)
    {
        var data = await _reportService.GetCustomerReportAsync(fromDate, toDate);
        var headers = new List<string> { "Customer Name", "Passport Number", "Bookings Count" };
        var rows = new List<List<object?>>();
        foreach (var c in data.MostFrequentCustomers)
        {
            rows.Add(new List<object?> { c.Name, c.PassportNumber, c.BookingsCount });
        }

        var rightAlignCols = new HashSet<int> { 1 };
        var textFormatCols = new HashSet<int> { 2 };

        byte[] fileContents = BusinessManagement.Web.Helpers.ExcelExportHelper.ExportToExcel(
            "Customer Report", headers, rows, rightAlignCols, textFormatCols);

        return File(fileContents, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "CustomerReport.xlsx");
    }

    [HttpGet]
    public async Task<IActionResult> ExportBookingExcel(DateTime? fromDate, DateTime? toDate)
    {
        var data = await _reportService.GetBookingReportAsync(fromDate, toDate);
        var headers = new List<string> { "Passenger Name", "Partner Name", "Travel Date", "Selling Price", "Total Cost", "Net Profit" };
        var rows = new List<List<object?>>();
        foreach (var b in data.DetailedBookings)
        {
            rows.Add(new List<object?> { b.PassengerName, b.PartnerName, b.TravelDate, b.SellingPrice, b.NetCost, b.Profit });
        }

        var rightAlignCols = new HashSet<int> { 1, 2 };

        byte[] fileContents = BusinessManagement.Web.Helpers.ExcelExportHelper.ExportToExcel(
            "Booking Report", headers, rows, rightAlignCols);

        return File(fileContents, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "BookingReport.xlsx");
    }

    [HttpGet]
    public async Task<IActionResult> ExportVisaExcel(DateTime? fromDate, DateTime? toDate)
    {
        var data = await _reportService.GetVisaReportAsync(fromDate, toDate);
        var headers = new List<string> { "Passenger Name", "Passport Number", "Visa Type", "Has QR", "Status" };
        var rows = new List<List<object?>>();
        foreach (var v in data.VisasList)
        {
            rows.Add(new List<object?> { v.PassengerName, v.PassportNumber, v.VisaType, v.HasQrCode ? "Yes" : "No", v.Status });
        }

        var rightAlignCols = new HashSet<int> { 1 };
        var textFormatCols = new HashSet<int> { 2 };

        byte[] fileContents = BusinessManagement.Web.Helpers.ExcelExportHelper.ExportToExcel(
            "Visa Report", headers, rows, rightAlignCols, textFormatCols);

        return File(fileContents, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "VisaReport.xlsx");
    }

    [HttpGet]
    public async Task<IActionResult> ExportCashExcel(DateTime? fromDate, DateTime? toDate)
    {
        var data = await _reportService.GetCashReportAsync(fromDate, toDate);
        var headers = new List<string> { "Date", "Reference", "Type", "Amount", "Currency", "Description" };
        var rows = new List<List<object?>>();
        foreach (var t in data.TransactionsList)
        {
            rows.Add(new List<object?> { t.Date, t.ReferenceNumber, t.Type, t.Amount, t.Currency, t.Description });
        }

        var rightAlignCols = new HashSet<int> { 6 };
        var textFormatCols = new HashSet<int> { 2 };

        byte[] fileContents = BusinessManagement.Web.Helpers.ExcelExportHelper.ExportToExcel(
            "Cash Report", headers, rows, rightAlignCols, textFormatCols);

        return File(fileContents, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "CashReport.xlsx");
    }

    [HttpGet]
    public async Task<IActionResult> ExportExpenseExcel(DateTime? fromDate, DateTime? toDate)
    {
        var data = await _reportService.GetExpenseReportAsync(fromDate, toDate);
        var headers = new List<string> { "Account Code", "Category", "Amount", "Percentage" };
        var rows = new List<List<object?>>();
        foreach (var e in data.ExpensesByCategory)
        {
            rows.Add(new List<object?> { e.AccountCode, e.CategoryName, e.Amount, $"{e.Percentage}%" });
        }

        var rightAlignCols = new HashSet<int> { 2 };
        var textFormatCols = new HashSet<int> { 1 };

        byte[] fileContents = BusinessManagement.Web.Helpers.ExcelExportHelper.ExportToExcel(
            "Expense Report", headers, rows, rightAlignCols, textFormatCols);

        return File(fileContents, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "ExpenseReport.xlsx");
    }

    [HttpGet]
    public async Task<IActionResult> ExportRevenueExcel(DateTime? fromDate, DateTime? toDate)
    {
        var data = await _reportService.GetRevenueReportAsync(fromDate, toDate);
        var headers = new List<string> { "Account Code", "Category", "Amount", "Percentage" };
        var rows = new List<List<object?>>();
        foreach (var r in data.RevenuesByCategory)
        {
            rows.Add(new List<object?> { r.AccountCode, r.CategoryName, r.Amount, $"{r.Percentage}%" });
        }

        var rightAlignCols = new HashSet<int> { 2 };
        var textFormatCols = new HashSet<int> { 1 };

        byte[] fileContents = BusinessManagement.Web.Helpers.ExcelExportHelper.ExportToExcel(
            "Revenue Report", headers, rows, rightAlignCols, textFormatCols);

        return File(fileContents, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "RevenueReport.xlsx");
    }

    [HttpGet]
    public async Task<IActionResult> ExportProfitExcel(DateTime? fromDate, DateTime? toDate)
    {
        var data = await _reportService.GetProfitReportAsync(fromDate, toDate);
        var headers = new List<string> { "Month", "Revenue", "Expense", "Net Profit" };
        var rows = new List<List<object?>>();
        foreach (var p in data.ProfitTimeline)
        {
            rows.Add(new List<object?> { p.MonthName, p.Revenue, p.Expense, p.Profit });
        }

        var rightAlignCols = new HashSet<int> { 1 };

        byte[] fileContents = BusinessManagement.Web.Helpers.ExcelExportHelper.ExportToExcel(
            "Profit Report", headers, rows, rightAlignCols);

        return File(fileContents, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "ProfitReport.xlsx");
    }

    [HttpGet]
    public async Task<IActionResult> ExportPartnerExcel()
    {
        var data = await _reportService.GetPartnerReportAsync();
        var headers = new List<string> { "Shareholder Name", "SAR Contribution", "EGP Contribution", "Profit Share Ratio", "Estimated Profit Quota" };
        var rows = new List<List<object?>>();
        foreach (var p in data)
        {
            rows.Add(new List<object?> { p.ShareholderName, p.AmountSar, p.AmountEgp, $"{p.ProfitShareRatio * 100}%", p.EstimatedProfitQuota });
        }

        var rightAlignCols = new HashSet<int> { 1 };

        byte[] fileContents = BusinessManagement.Web.Helpers.ExcelExportHelper.ExportToExcel(
            "Partner Report", headers, rows, rightAlignCols);

        return File(fileContents, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "PartnerReport.xlsx");
    }

    [HttpGet]
    public async Task<IActionResult> ExportPeriodicExcel(string rangeType, int year)
    {
        var data = await _reportService.GetPeriodicReportsAsync(rangeType, year);
        var headers = new List<string> { "Period", "Bookings Count", "Revenues", "Expenses", "Net Profit" };
        var rows = new List<List<object?>>();
        foreach (var p in data)
        {
            rows.Add(new List<object?> { p.PeriodLabel, p.BookingsCount, p.Revenues, p.Expenses, p.NetProfit });
        }

        var rightAlignCols = new HashSet<int> { 1 };

        byte[] fileContents = BusinessManagement.Web.Helpers.ExcelExportHelper.ExportToExcel(
            "Periodic Report", headers, rows, rightAlignCols);

        return File(fileContents, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "PeriodicReport.xlsx");
    }
}
