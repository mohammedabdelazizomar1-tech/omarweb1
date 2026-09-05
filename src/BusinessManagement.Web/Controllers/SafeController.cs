using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using BusinessManagement.Application.DTOs;
using BusinessManagement.Application.Interfaces;

namespace BusinessManagement.Web.Controllers;

[Authorize(Policy = "safe.view")]
public class SafeController : Controller
{
    private readonly ISafeService _safeService;
    private readonly IBookingService _bookingService;
    private readonly ILogger<SafeController> _logger;

    public SafeController(
        ISafeService safeService,
        IBookingService bookingService,
        ILogger<SafeController> logger)
    {
        _safeService = safeService;
        _bookingService = bookingService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        ViewData["PageHeader"] = "خزينة المعاملات المالية";
        _logger.LogInformation("Loading cash book ledger. User: {User}", User.Identity?.Name);
        
        try
        {
            var transactions = await _safeService.GetAllTransactionsAsync();
            var balances = await _safeService.GetSafeBalancesAsync();
            var partners = await _bookingService.GetPartnersLookupAsync();
            
            var allBookings = await _bookingService.GetAllBookingsAsync();
            var activeBookings = allBookings
                .Where(b => b.RemainingBalance > 0)
                .OrderBy(b => b.BookingNumber)
                .ToList();

            ViewBag.BalanceEgp = balances.Egp;
            ViewBag.BalanceSar = balances.Sar;
            ViewBag.Partners = partners;
            ViewBag.ActiveBookings = activeBookings;

            return View(transactions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred loading cash safe ledger page.");
            ViewBag.BalanceEgp = 0m;
            ViewBag.BalanceSar = 0m;
            ViewBag.Partners = new List<PartnerQuotaMetric>();
            return View(new List<SafeTransactionDto>());
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateSafeTransactionDto model)
    {
        if (!ModelState.IsValid)
        {
            var errors = string.Join(" | ", ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage));
            return Json(new { success = false, message = $"Input validation failed: {errors}" });
        }

        try
        {
            await _safeService.CreateTransactionAsync(model);
            return Json(new { success = true, message = "Safe cash flow logged successfully!" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating safe transaction.");
            return Json(new { success = false, message = "An error occurred while logging the safe transaction." });
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(Guid id)
    {
        _logger.LogWarning("Initiating deletion of safe transaction ID: {TransactionId} by user {User}", id, User.Identity?.Name);
        try
        {
            await _safeService.DeleteTransactionAsync(id);
            return Json(new { success = true, message = "Safe transaction record deleted successfully!" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting safe transaction: {TransactionId}", id);
            return Json(new { success = false, message = ex.Message });
        }
    }

    [HttpGet]
    public async Task<IActionResult> Print(Guid id)
    {
        _logger.LogInformation("Generating receipt for safe transaction ID: {TransactionId}", id);
        try
        {
            var transaction = await _safeService.GetTransactionByIdAsync(id);
            if (transaction == null)
            {
                return NotFound("سجل الحركة المالية غير موجود.");
            }
            return View(transaction);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error printing safe transaction receipt: {TransactionId}", id);
            return BadRequest("حدث خطأ أثناء تحميل تفاصيل السند.");
        }
    }

    [HttpGet]
    public async Task<IActionResult> PrintBulk(string ids)
    {
        if (string.IsNullOrEmpty(ids))
        {
            return BadRequest("لم يتم تحديد أي حركات للطباعة.");
        }

        var idList = ids.Split(',', StringSplitOptions.RemoveEmptyEntries)
                        .Select(x => Guid.TryParse(x, out var guid) ? guid : Guid.Empty)
                        .Where(x => x != Guid.Empty)
                        .ToList();

        var transactionsList = new List<BusinessManagement.Application.DTOs.SafeTransactionDto>();
        foreach (var id in idList)
        {
            var tx = await _safeService.GetTransactionByIdAsync(id);
            if (tx != null)
            {
                transactionsList.Add(tx);
            }
        }

        return View(transactionsList);
    }
}





