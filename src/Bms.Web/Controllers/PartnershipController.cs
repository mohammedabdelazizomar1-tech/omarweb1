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
public class PartnershipController : Controller
{
    private readonly IPartnershipService _partnershipService;
    private readonly ILogger<PartnershipController> _logger;

    public PartnershipController(IPartnershipService partnershipService, ILogger<PartnershipController> logger)
    {
        _partnershipService = partnershipService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        ViewData["PageHeader"] = "رأس مال الشركاء (الضمان)";
        _logger.LogInformation("Loading partnership capital directory. User: {User}", User.Identity?.Name);

        try
        {
            var capitals = await _partnershipService.GetAllCapitalAsync();
            var profitShares = await _partnershipService.CalculateProfitSharesAsync();

            ViewBag.ProfitShares = profitShares;

            return View(capitals);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred loading partnership capital structure.");
            ViewBag.ProfitShares = new List<PartnerQuotaMetric>();
            return View(new List<PartnershipCapitalDto>());
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreatePartnershipCapitalDto model)
    {
        if (!ModelState.IsValid)
        {
            return Json(new { success = false, message = "Input validation failed. Please check capital fields." });
        }

        try
        {
            await _partnershipService.CreateCapitalAsync(model);
            return Json(new { success = true, message = "Shareholder capital investment logged successfully!" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error registering partner capital.");
            return Json(new { success = false, message = "An error occurred while logging shareholder investment." });
        }
    }
}
