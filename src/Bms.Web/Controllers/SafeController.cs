using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Bms.Application.DTOs;
using Bms.Application.Interfaces;

namespace Bms.Web.Controllers;

[Authorize]
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

            ViewBag.BalanceEgp = balances.Egp;
            ViewBag.BalanceSar = balances.Sar;
            ViewBag.Partners = partners;

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
            return Json(new { success = false, message = "Input validation failed. Please check cash ledger parameters." });
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
}
