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
public class ExternalVisasController : Controller
{
    private readonly IExternalVisaService _visaService;
    private readonly ILogger<ExternalVisasController> _logger;

    public ExternalVisasController(IExternalVisaService visaService, ILogger<ExternalVisasController> logger)
    {
        _visaService = visaService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        ViewData["PageHeader"] = "سجل تأشيرات خارجي (الطرف الثالث)";
        _logger.LogInformation("Loading external visas directory. User: {User}", User.Identity?.Name);

        try
        {
            var visas = await _visaService.GetAllVisasAsync();
            return View(visas);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred loading external visas list.");
            return View(new List<ExternalVisaDto>());
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateExternalVisaDto model)
    {
        if (!ModelState.IsValid)
        {
            return Json(new { success = false, message = "Input validation failed. Please check cost parameters." });
        }

        try
        {
            await _visaService.CreateVisaAsync(model);
            return Json(new { success = true, message = "External visa file registered successfully!" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error registering external visa file.");
            return Json(new { success = false, message = "An error occurred while logging the external visa record." });
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(Guid id)
    {
        _logger.LogWarning("Deleting external visa file ID: {VisaId}", id);
        try
        {
            await _visaService.DeleteVisaAsync(id);
            return Json(new { success = true, message = "External visa record deleted successfully!" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting external visa file: {VisaId}", id);
            return Json(new { success = false, message = "An error occurred while deleting the external visa record." });
        }
    }
}
