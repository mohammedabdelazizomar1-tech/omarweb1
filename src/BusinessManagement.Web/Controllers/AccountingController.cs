using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using BusinessManagement.Application.DTOs;
using BusinessManagement.Application.Interfaces;
using BusinessManagement.Web.Filters;

namespace BusinessManagement.Web.Controllers;

[Authorize(Roles = "Admin,Manager")]
public class AccountingController : Controller
{
    private readonly IAccountingService _accountingService;
    private readonly ISecurityAuditService _auditService;
    private readonly ILogger<AccountingController> _logger;

    public AccountingController(
        IAccountingService accountingService,
        ISecurityAuditService auditService,
        ILogger<AccountingController> logger)
    {
        _accountingService = accountingService;
        _auditService = auditService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var accounts = await _accountingService.GetChartOfAccountsAsync();
        var journalEntries = await _accountingService.GetJournalEntriesAsync();
        var closings = await _accountingService.GetMonthlyClosingsAsync();

        ViewBag.SuccessMessage = TempData["SuccessMessage"];
        ViewBag.ErrorMessage = TempData["ErrorMessage"];

        // Extract cash box balances EGP/SAR
        ViewBag.CashEgp = accounts.FirstOrDefault(a => a.AccountCode == "1101")?.CurrentBalance ?? 0;
        ViewBag.CashSar = accounts.FirstOrDefault(a => a.AccountCode == "1102")?.CurrentBalance ?? 0;
        ViewBag.TotalAssets = accounts.Where(a => a.Type == "Asset").Sum(a => a.CurrentBalance);

        return View(journalEntries.Take(10));
    }

    [HttpGet]
    public async Task<IActionResult> ChartOfAccounts()
    {
        var accounts = await _accountingService.GetChartOfAccountsAsync();
        return View(accounts);
    }

    [HttpGet]
    public async Task<IActionResult> JournalEntries()
    {
        var entries = await _accountingService.GetJournalEntriesAsync();
        return View(entries);
    }

    [HttpGet]
    public async Task<IActionResult> CreateJournalEntry()
    {
        var accounts = await _accountingService.GetChartOfAccountsAsync();
        ViewBag.Accounts = accounts.Where(a => a.IsActive).ToList();
        return View(new CreateJournalEntryDto { EntryDate = DateOnly.FromDateTime(DateTime.Today) });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateJournalEntry([FromBody] CreateJournalEntryDto model)
    {
        if (model == null || model.Lines == null || !model.Lines.Any())
        {
            return Json(new { success = false, message = "بيانات قيد اليومية غير صالحة أو فارغة." });
        }

        try
        {
            string user = User.Identity?.Name ?? "system";
            var created = await _accountingService.CreateJournalEntryAsync(model, user);

            string userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "system";
            Guid.TryParse(userId, out var guidUserId);
            await _auditService.LogActivityAsync(guidUserId, $"Create Journal Entry: {created.ReferenceNumber}", HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown", Request.Headers["User-Agent"].ToString());

            return Json(new { success = true, message = $"تم تسجيل القيد بنجاح بالرقم {created.ReferenceNumber}!" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating journal entry");
            return Json(new { success = false, message = ex.Message });
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> PostJournalEntry(Guid id)
    {
        try
        {
            bool success = await _accountingService.PostJournalEntryAsync(id);
            if (success)
            {
                string userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "system";
                Guid.TryParse(userId, out var guidUserId);
                await _auditService.LogActivityAsync(guidUserId, $"Post/Approve Journal Entry ID: {id}", HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown", Request.Headers["User-Agent"].ToString());

                return Json(new { success = true, message = "تم ترحيل واعتماد القيد بنجاح!" });
            }
            return Json(new { success = false, message = "فشل ترحيل القيد." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error posting journal entry");
            return Json(new { success = false, message = ex.Message });
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteJournalEntry(Guid id)
    {
        try
        {
            bool success = await _accountingService.DeleteJournalEntryAsync(id);
            if (success)
            {
                return Json(new { success = true, message = "تم حذف مسودة القيد بنجاح." });
            }
            return Json(new { success = false, message = "فشل حذف القيد." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting journal entry");
            return Json(new { success = false, message = ex.Message });
        }
    }

    [HttpGet]
    public async Task<IActionResult> Ledger(Guid? accountId, string? fromDate, string? toDate)
    {
        var accounts = await _accountingService.GetChartOfAccountsAsync();
        ViewBag.Accounts = accounts.ToList();

        if (!accountId.HasValue && accounts.Any())
        {
            accountId = accounts.First().Id;
        }

        if (!accountId.HasValue)
        {
            return View(new LedgerDto());
        }

        DateOnly? fDate = string.IsNullOrWhiteSpace(fromDate) ? null : DateOnly.Parse(fromDate);
        DateOnly? tDate = string.IsNullOrWhiteSpace(toDate) ? null : DateOnly.Parse(toDate);

        ViewBag.SelectedAccountId = accountId.Value;
        ViewBag.FromDate = fromDate;
        ViewBag.ToDate = toDate;

        var ledger = await _accountingService.GetGeneralLedgerAsync(accountId.Value, fDate, tDate);
        return View(ledger);
    }

    [HttpGet]
    public async Task<IActionResult> IncomeStatement(string? fromDate, string? toDate)
    {
        DateOnly fDate = string.IsNullOrWhiteSpace(fromDate) ? new DateOnly(DateTime.Today.Year, DateTime.Today.Month, 1) : DateOnly.Parse(fromDate);
        DateOnly tDate = string.IsNullOrWhiteSpace(toDate) ? DateOnly.FromDateTime(DateTime.Today) : DateOnly.Parse(toDate);

        ViewBag.FromDate = fDate.ToString("yyyy-MM-dd");
        ViewBag.ToDate = tDate.ToString("yyyy-MM-dd");

        var report = await _accountingService.GetIncomeStatementAsync(fDate, tDate);
        return View(report);
    }

    [HttpGet]
    public async Task<IActionResult> BalanceSheet(string? asOfDate)
    {
        DateOnly limitDate = string.IsNullOrWhiteSpace(asOfDate) ? DateOnly.FromDateTime(DateTime.Today) : DateOnly.Parse(asOfDate);
        ViewBag.AsOfDate = limitDate.ToString("yyyy-MM-dd");

        var report = await _accountingService.GetBalanceSheetAsync(limitDate);
        return View(report);
    }

    [HttpGet]
    public IActionResult DistributeProfit()
    {
        return View(new DistributeProfitDto());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DistributeProfit(DistributeProfitDto model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            string user = User.Identity?.Name ?? "system";
            bool success = await _accountingService.DistributePartnersProfitAsync(model, user);

            if (success)
            {
                TempData["SuccessMessage"] = $"تم توزيع أرباح الشراكة بقيمة {model.Amount:N2} {model.Currency} بنجاح وقيد حركاتها المزدوجة!";
                return RedirectToAction("Index");
            }

            ModelState.AddModelError(string.Empty, "حدث خطأ أثناء توزيع الأرباح. تأكد من وجود رأس مال نشط للشركاء.");
            return View(model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error distributing partner profits");
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(model);
        }
    }

    [HttpGet]
    public async Task<IActionResult> MonthlyClosing()
    {
        var closings = await _accountingService.GetMonthlyClosingsAsync();
        ViewBag.SuccessMessage = TempData["SuccessMessage"];
        ViewBag.ErrorMessage = TempData["ErrorMessage"];
        return View(closings);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CloseMonth(int year, int month)
    {
        string user = User.Identity?.Name ?? "system";
        await _accountingService.CloseMonthAsync(year, month, user);
        TempData["SuccessMessage"] = $"تم إقفال الشهر {month}/{year} محاسبياً بنجاح وتجميد الحركات المالية فيه.";
        return RedirectToAction("MonthlyClosing");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ReopenMonth(int year, int month)
    {
        await _accountingService.ReopenMonthAsync(year, month);
        TempData["SuccessMessage"] = $"تم إعادة فتح الشهر {month}/{year} محاسبياً وفك تجميد الحركات.";
        return RedirectToAction("MonthlyClosing");
    }
}
