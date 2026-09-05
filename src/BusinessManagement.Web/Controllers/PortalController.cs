using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using BusinessManagement.Application.DTOs;
using BusinessManagement.Application.Interfaces;

namespace BusinessManagement.Web.Controllers;

[Authorize]
public class PortalController : Controller
{
    private readonly IGatewayService _gatewayService;
    private readonly IBookingService _bookingService;
    private readonly ILogger<PortalController> _logger;

    public PortalController(
        IGatewayService gatewayService,
        IBookingService bookingService,
        ILogger<PortalController> logger)
    {
        _gatewayService = gatewayService;
        _bookingService = bookingService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        ViewData["PageHeader"] = "بوابات التأشيرات والمنافذ (BW)";
        _logger.LogInformation("Loading portal gateways management page. User: {User}", User.Identity?.Name);

        try
        {
            var purchases = await _gatewayService.GetAllPortalPurchasesAsync();
            var balances = await _gatewayService.GetPortalBalancesAsync();
            var partners = await _bookingService.GetPartnersLookupAsync();

            ViewBag.AvailableQr = balances.QrCount;
            ViewBag.AvailableVip = balances.VipCount;
            ViewBag.Partners = partners;

            return View(purchases);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred loading gateway portal logs.");
            ViewBag.AvailableQr = 0;
            ViewBag.AvailableVip = 0;
            ViewBag.Partners = new List<PartnerQuotaMetric>();
            return View(new List<GatewayPortalDto>());
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateGatewayPortalDto model)
    {
        if (!ModelState.IsValid)
        {
            return Json(new { success = false, message = "Input validation failed. Please check form parameters." });
        }

        try
        {
            await _gatewayService.CreatePortalPurchaseAsync(model);
            return Json(new { success = true, message = "Portal package registered successfully!" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error registering portal package purchase.");
            return Json(new { success = false, message = "An error occurred while logging portal purchases." });
        }
    }
}





