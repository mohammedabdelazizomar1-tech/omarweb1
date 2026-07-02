using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Bms.Application.DTOs;
using Bms.Application.Interfaces;

namespace Bms.Web.Controllers;

[Authorize]
public class BookingsController : Controller
{
    private readonly IBookingService _bookingService;
    private readonly ILogger<BookingsController> _logger;

    public BookingsController(IBookingService bookingService, ILogger<BookingsController> logger)
    {
        _bookingService = bookingService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        ViewData["PageHeader"] = "دفتر حجوزات المسافرين";
        string? partnerUsername = User.IsInRole("Partner") ? User.Identity?.Name : null;
        _logger.LogInformation("Loading bookings log. Partner filter: {Partner}", partnerUsername ?? "All");
        
        try
        {
            var bookings = await _bookingService.GetAllBookingsAsync(partnerUsername);
            return View(bookings);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred loading bookings directory.");
            return View(new List<BookingDto>());
        }
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        ViewData["PageHeader"] = "تسجيل حجز مسافر جديد";
        _logger.LogInformation("Loading traveler booking form.");
        try
        {
            var passengers = await _bookingService.GetAllPassengersAsync();
            var partners = await _bookingService.GetPartnersLookupAsync();

            ViewBag.Passengers = passengers;
            ViewBag.Partners = partners;

            return View(new CreateBookingDto { TravelDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(30)) });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading traveler form assets.");
            return RedirectToAction("Index");
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateBookingDto model)
    {
        if (!ModelState.IsValid)
        {
            return Json(new { success = false, message = "Input validation failed. Please check cost fields." });
        }

        try
        {
            // If logged user is a Partner, force booking attribution to themselves
            if (User.IsInRole("Partner"))
            {
                var partners = await _bookingService.GetPartnersLookupAsync();
                var currentPartner = partners.FirstOrDefault(p => p.PartnerName.Equals(User.Identity?.Name, StringComparison.OrdinalIgnoreCase));
                if (currentPartner != null)
                {
                    model.PartnerId = currentPartner.PartnerId;
                }
            }

            var booking = await _bookingService.CreateBookingAsync(model);
            return Json(new { success = true, message = "Traveler booking registered successfully!", bookingId = booking.Id });
        }
        catch (InvalidOperationException ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating booking file.");
            return Json(new { success = false, message = "An internal server error occurred while writing booking." });
        }
    }

    [HttpGet]
    public async Task<IActionResult> Edit(Guid id)
    {
        ViewData["PageHeader"] = "تعديل تفاصيل ملف الحجز";
        _logger.LogInformation("Retrieving booking edit form for ID: {BookingId}", id);
        try
        {
            var booking = await _bookingService.GetBookingByIdAsync(id);
            if (booking == null)
            {
                return NotFound("Booking file not found.");
            }

            // Partner-restricted access check
            if (User.IsInRole("Partner") && !booking.PartnerName.Equals(User.Identity?.Name, StringComparison.OrdinalIgnoreCase))
            {
                return Forbid("You do not have permission to modify this partner's booking.");
            }

            var passengers = await _bookingService.GetAllPassengersAsync();
            var partners = await _bookingService.GetPartnersLookupAsync();

            ViewBag.Passengers = passengers;
            ViewBag.Partners = partners;

            var model = new CreateBookingDto
            {
                PassengerId = booking.PassengerId,
                PartnerId = booking.PartnerId,
                TravelDate = booking.TravelDate,
                NetCost = booking.NetCost,
                BarcodeCost = booking.BarcodeCost,
                CompanyCost = booking.CompanyCost,
                AirportCost = booking.AirportCost,
                ProgramCost = booking.ProgramCost,
                TicketCost = booking.TicketCost,
                BusCost = booking.BusCost,
                SellingPrice = booking.SellingPrice,
                PaymentsCollected = booking.PaymentsCollected,
                HasQrCode = booking.HasQrCode,
                Notes = booking.Notes
            };

            return View(model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading booking edit page for ID: {BookingId}", id);
            return RedirectToAction("Index");
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, CreateBookingDto model)
    {
        if (!ModelState.IsValid)
        {
            return Json(new { success = false, message = "Input validation failed. Please check cost fields." });
        }

        try
        {
            var booking = await _bookingService.GetBookingByIdAsync(id);
            if (booking == null)
            {
                return Json(new { success = false, message = "Booking record not found." });
            }

            if (User.IsInRole("Partner") && !booking.PartnerName.Equals(User.Identity?.Name, StringComparison.OrdinalIgnoreCase))
            {
                return Json(new { success = false, message = "Access denied. You cannot alter other partners' files." });
            }

            await _bookingService.UpdateBookingAsync(id, model);
            return Json(new { success = true, message = "Booking record updated successfully!" });
        }
        catch (InvalidOperationException ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating booking: {BookingId}", id);
            return Json(new { success = false, message = "An error occurred while saving booking updates." });
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(Guid id)
    {
        _logger.LogWarning("Initiating deletion of booking: {BookingId} by user {User}", id, User.Identity?.Name);
        try
        {
            var booking = await _bookingService.GetBookingByIdAsync(id);
            if (booking == null)
            {
                return Json(new { success = false, message = "Booking record not found." });
            }

            if (User.IsInRole("Partner") && !booking.PartnerName.Equals(User.Identity?.Name, StringComparison.OrdinalIgnoreCase))
            {
                return Json(new { success = false, message = "Access denied. You cannot delete other partners' files." });
            }

            await _bookingService.DeleteBookingAsync(id);
            return Json(new { success = true, message = "Booking record deleted successfully!" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting booking: {BookingId}", id);
            return Json(new { success = false, message = "An error occurred while deleting the booking record." });
        }
    }

    #region Passenger AJAX Handles
    [HttpGet]
    public async Task<IActionResult> Passengers()
    {
        ViewData["PageHeader"] = "قاعدة بيانات المسافرين (CRM)";
        var passengers = await _bookingService.GetAllPassengersAsync();
        return View(passengers);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreatePassenger(CreatePassengerDto model)
    {
        if (!ModelState.IsValid)
        {
            var errors = string.Join(" ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
            return Json(new { success = false, message = errors });
        }

        try
        {
            var passenger = await _bookingService.CreatePassengerAsync(model);
            return Json(new { success = true, message = "Passenger registered successfully!", passengerId = passenger.Id, name = passenger.FullName, passport = passenger.PassportNumber });
        }
        catch (InvalidOperationException ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding passenger.");
            return Json(new { success = false, message = "An error occurred while adding the passenger profile." });
        }
    }
    #endregion
}
