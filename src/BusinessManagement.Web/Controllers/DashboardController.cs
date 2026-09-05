using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using BusinessManagement.Application.DTOs;
using BusinessManagement.Application.Interfaces;

namespace BusinessManagement.Web.Controllers;

[Authorize]
public class DashboardController : Controller
{
    private readonly ITravelDashboardService _dashboardService;
    private readonly ILogger<DashboardController> _logger;

    public DashboardController(ITravelDashboardService dashboardService, ILogger<DashboardController> logger)
    {
        _dashboardService = dashboardService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        ViewData["PageHeader"] = "لوحة تحكم عمليات الحج والعمرة";
        string? partnerUsername = User.IsInRole("Partner") ? User.Identity?.Name : null;
        _logger.LogInformation("Loading Hajj & Umrah dashboard statistics. Partner filter: {Partner}", partnerUsername ?? "Admin");
        
        try
        {
            var summary = await _dashboardService.GetDashboardDataAsync(partnerUsername);
            return View(summary);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while rendering dashboard metrics.");
            return View(new TravelDashboardDto());
        }
    }
}





