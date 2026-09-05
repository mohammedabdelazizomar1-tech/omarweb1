using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using BusinessManagement.Application.DTOs;
using BusinessManagement.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using OfficeOpenXml;

namespace BusinessManagement.Web.Controllers;

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
    [Authorize(Policy = "bookings.view")]
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
    [Authorize(Policy = "bookings.create")]
    public async Task<IActionResult> Create(Guid? passengerId)
    {
        ViewData["PageHeader"] = "تسجيل حجز مسافر جديد";
        _logger.LogInformation("Loading traveler booking form.");
        try
        {
            var passengers = await _bookingService.GetAllPassengersAsync();
            var bookings = await _bookingService.GetAllBookingsAsync();
            var bookedPassengerIds = new System.Collections.Generic.HashSet<Guid>(
                System.Linq.Enumerable.Select(bookings, b => b.PassengerId)
            );

            var availablePassengers = System.Linq.Enumerable.ToList(
                System.Linq.Enumerable.Where(passengers, p => !bookedPassengerIds.Contains(p.Id) || (passengerId.HasValue && p.Id == passengerId.Value))
            );

            var partners = await _bookingService.GetPartnersLookupAsync();

            ViewBag.Passengers = availablePassengers;
            ViewBag.Partners = partners;

            var model = new CreateBookingDto 
            { 
                TravelDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(30)),
                PassengerId = passengerId
            };
            return View(model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading traveler form assets.");
            return RedirectToAction("Index");
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = "bookings.create")]
    public async Task<IActionResult> Create(CreateBookingDto model)
    {
        if (User.IsInRole("Partner"))
        {
            var partners = await _bookingService.GetPartnersLookupAsync();
            var currentPartner = partners.FirstOrDefault(p => p.PartnerName.Equals(User.Identity?.Name, StringComparison.OrdinalIgnoreCase));
            if (currentPartner != null)
            {
                model.PartnerId = currentPartner.PartnerId;
                ModelState.Remove(nameof(model.PartnerId));
            }
        }

        // Manual validation for passengers choice
        if ((model.PassengerId == null || model.PassengerId == Guid.Empty) && (model.PassengerIds == null || !model.PassengerIds.Any()))
        {
            return Json(new { success = false, message = "يجب اختيار مسافر واحد على الأقل." });
        }

        if (!ModelState.IsValid)
        {
            var errors = string.Join(" ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
            return Json(new { success = false, message = $"Input validation failed: {errors}" });
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

            if (model.PassengerIds != null && model.PassengerIds.Any())
            {
                string? groupNumber = null;
                if (model.PassengerIds.Count > 1 && model.RegisterAsGroup)
                {
                    // Generate a beautiful, unique short group number, e.g. GRP-A1B2C3
                    groupNumber = $"GRP-{Guid.NewGuid().ToString("N").Substring(0, 6).ToUpper()}";
                }

                var createdBookingIds = new List<Guid>();
                foreach (var passengerId in model.PassengerIds)
                {
                    decimal? initialPayment = null;
                    if (model.PassengerPayments != null && model.PassengerPayments.TryGetValue(passengerId, out var amt))
                    {
                        initialPayment = amt; // can be null if empty string submitted
                    }
                    else
                    {
                        initialPayment = model.PaymentsCollected;
                    }

                    string? paymentMethod = null;
                    if (model.PassengerPaymentMethods != null && model.PassengerPaymentMethods.TryGetValue(passengerId, out var method))
                    {
                        paymentMethod = method;
                    }
                    if (string.IsNullOrEmpty(paymentMethod))
                    {
                        paymentMethod = model.PaymentMethod ?? "Cash";
                    }

                    string? receiptNumber = null;
                    if (model.PassengerReceiptNumbers != null && model.PassengerReceiptNumbers.TryGetValue(passengerId, out var receipt))
                    {
                        receiptNumber = receipt;
                    }
                    if (string.IsNullOrEmpty(receiptNumber))
                    {
                        receiptNumber = model.ReceiptNumber;
                    }

                    DateOnly? paymentDate = null;
                    if (model.PassengerPaymentDates != null && model.PassengerPaymentDates.TryGetValue(passengerId, out var pDate))
                    {
                        paymentDate = pDate;
                    }
                    if (!paymentDate.HasValue)
                    {
                        paymentDate = model.PaymentDate ?? DateOnly.FromDateTime(DateTime.UtcNow);
                    }

                    var singleModel = new CreateBookingDto
                    {
                        PassengerId = passengerId,
                        PartnerId = model.PartnerId,
                        TravelDate = model.TravelDate,
                        NetCost = model.NetCost,
                        BarcodeCost = model.BarcodeCost,
                        CompanyCost = model.CompanyCost,
                        AirportCost = model.AirportCost,
                        ProgramCost = model.ProgramCost,
                        TicketCost = model.TicketCost,
                        BusCost = model.BusCost,
                        SellingPrice = model.SellingPrice,
                        PaymentsCollected = initialPayment,
                        PaymentMethod = paymentMethod,
                        ReceiptNumber = receiptNumber,
                        PaymentDate = paymentDate,
                        Notes = model.Notes,
                        HasQrCode = model.HasQrCode,
                        GroupNumber = groupNumber
                    };
                    var b = await _bookingService.CreateBookingAsync(singleModel);
                    createdBookingIds.Add(b.Id);
                }
                return Json(new { success = true, message = $"تم تسجيل عدد {model.PassengerIds.Count} حجز للمسافرين المحددين بنجاح!" });
            }
            else
            {
                var booking = await _bookingService.CreateBookingAsync(model);
                return Json(new { success = true, message = "Traveler booking registered successfully!", bookingId = booking.Id });
            }
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
    [Authorize(Policy = "bookings.edit")]
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
    [Authorize(Policy = "bookings.edit")]
    public async Task<IActionResult> Edit(Guid id, CreateBookingDto model)
    {
        if (User.IsInRole("Partner"))
        {
            var partners = await _bookingService.GetPartnersLookupAsync();
            var currentPartner = partners.FirstOrDefault(p => p.PartnerName.Equals(User.Identity?.Name, StringComparison.OrdinalIgnoreCase));
            if (currentPartner != null)
            {
                model.PartnerId = currentPartner.PartnerId;
                ModelState.Remove(nameof(model.PartnerId));
            }
        }

        if (!ModelState.IsValid)
        {
            var errors = string.Join(" ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
            return Json(new { success = false, message = $"Input validation failed: {errors}" });
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
    [Authorize(Policy = "bookings.delete")]
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
            return Json(new { success = true, message = "Passenger registered successfully!", passengerId = passenger.Id, name = passenger.FullName, passport = passenger.PassportNumber, partnerId = passenger.PartnerId });
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

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeletePassenger(Guid id)
    {
        _logger.LogWarning("Initiating deletion of passenger: {PassengerId} by user {User}", id, User.Identity?.Name);
        try
        {
            await _bookingService.DeletePassengerAsync(id);
            return Json(new { success = true, message = "Passenger profile deleted successfully!" });
        }
        catch (InvalidOperationException ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting passenger: {PassengerId}", id);
            return Json(new { success = false, message = "An error occurred while deleting the passenger profile." });
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ImportPassengers(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return Json(new { success = false, message = "Please upload a valid Excel file." });
        }

        // Validate file security (Point 6/v2 & Point 5/v4)
        using (var stream = file.OpenReadStream())
        {
            var validation = BusinessManagement.Shared.Security.FileSecurityValidator.ValidateFile(stream, file.FileName, file.ContentType, file.Length);
            if (!validation.IsValid)
            {
                return Json(new { success = false, message = validation.ErrorMessage });
            }
        }

        try
        {
            using var memoryStream = new MemoryStream();
            await file.CopyToAsync(memoryStream);
            memoryStream.Position = 0;
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using var package = new ExcelPackage(memoryStream);
            var worksheet = package.Workbook.Worksheets.FirstOrDefault();
            if (worksheet == null)
            {
                return Json(new { success = false, message = "Excel file does not contain a valid worksheet." });
            }

            var partnersLookup = await _bookingService.GetPartnersLookupAsync();
            var rows = worksheet.Dimension?.Rows ?? 0;
            var models = new List<CreatePassengerDto>();

            for (int row = 2; row <= rows; row++)
            {
                var fullName = worksheet.Cells[row, 1].Text?.Trim() ?? string.Empty;
                var partnerStr = worksheet.Cells[row, 2].Text?.Trim() ?? string.Empty;
                var passportNumber = worksheet.Cells[row, 3].Text?.Trim() ?? string.Empty;

                if (string.IsNullOrWhiteSpace(fullName) && 
                    string.IsNullOrWhiteSpace(partnerStr) && 
                    string.IsNullOrWhiteSpace(passportNumber))
                {
                    continue;
                }

                if (string.IsNullOrWhiteSpace(fullName))
                {
                    return Json(new { success = false, message = $"الصف {row}: اسم المسافر مطلوب ولا يمكن أن يكون فارغاً." });
                }

                Guid? partnerId = null;
                if (!string.IsNullOrWhiteSpace(partnerStr))
                {
                    var normPartnerStr = partnerStr.Trim().ToLower();
                    if (normPartnerStr == "mohammed") normPartnerStr = "mohamed";

                    var matchedPartner = partnersLookup.FirstOrDefault(pt => 
                        pt.PartnerName.Trim().Equals(normPartnerStr, StringComparison.OrdinalIgnoreCase) ||
                        pt.PartnerName.Trim().Equals(partnerStr, StringComparison.OrdinalIgnoreCase)
                    );

                    if (matchedPartner != null)
                    {
                        partnerId = matchedPartner.PartnerId;
                    }
                    else
                    {
                        return Json(new { success = false, message = $"الصف {row}: التبعية لـ ({partnerStr}) غير صالحة. يجب أن تكون أحد الشركاء: omar - khaled - mohammed - alaa." });
                    }
                }
                else
                {
                    return Json(new { success = false, message = $"الصف {row}: حقل التبعية مطلوب للمسافر ({fullName})." });
                }

                if (string.IsNullOrWhiteSpace(passportNumber) || !System.Text.RegularExpressions.Regex.IsMatch(passportNumber, "^[aA]\\d{7}$"))
                {
                    return Json(new { success = false, message = $"الصف {row}: رقم جواز السفر غير صالح لـ ({fullName}). يجب أن يبدأ بحرف A ويليه 7 أرقام (مثال: A1234567)." });
                }

                var passportExpiryCell = worksheet.Cells[row, 4];
                var nationalId = worksheet.Cells[row, 5].Text?.Trim() ?? string.Empty;
                var dateOfBirthCell = worksheet.Cells[row, 6];
                var nationality = worksheet.Cells[row, 7].Text?.Trim() ?? string.Empty;
                var gender = worksheet.Cells[row, 8].Text?.Trim() ?? string.Empty;
                var phoneNumber = worksheet.Cells[row, 9].Text?.Trim() ?? string.Empty;
                var email = worksheet.Cells[row, 10].Text?.Trim() ?? string.Empty;
                var address = worksheet.Cells[row, 11].Text?.Trim() ?? string.Empty;
                var emergencyName = worksheet.Cells[row, 12].Text?.Trim() ?? string.Empty;
                var emergencyPhone = worksheet.Cells[row, 13].Text?.Trim() ?? string.Empty;

                if (!string.IsNullOrWhiteSpace(nationalId) && !System.Text.RegularExpressions.Regex.IsMatch(nationalId, "^\\d{14}$"))
                {
                    return Json(new { success = false, message = $"الصف {row}: الرقم القومي لـ ({fullName}) غير صالح. يجب أن يتكون من 14 رقماً." });
                }

                DateOnly? passportExpiry = null;
                if (passportExpiryCell.Value != null)
                {
                    var expiryText = passportExpiryCell.Text?.Trim();
                    if (passportExpiryCell.Value is DateTime cellDateTime)
                    {
                        passportExpiry = DateOnly.FromDateTime(cellDateTime);
                    }
                    else if (DateOnly.TryParse(expiryText, out var parsedExpiry))
                    {
                        passportExpiry = parsedExpiry;
                    }
                    else if (DateTime.TryParse(expiryText, out var parsedDateTime))
                    {
                        passportExpiry = DateOnly.FromDateTime(parsedDateTime);
                    }
                    else if (double.TryParse(expiryText, out var oaDate))
                    {
                        passportExpiry = DateOnly.FromDateTime(DateTime.FromOADate(oaDate));
                    }
                }

                DateOnly? dateOfBirth = null;
                if (dateOfBirthCell.Value != null)
                {
                    var dobText = dateOfBirthCell.Text?.Trim();
                    if (dateOfBirthCell.Value is DateTime cellDateTime)
                    {
                        dateOfBirth = DateOnly.FromDateTime(cellDateTime);
                    }
                    else if (DateOnly.TryParse(dobText, out var parsedDob))
                    {
                        dateOfBirth = parsedDob;
                    }
                    else if (DateTime.TryParse(dobText, out var parsedDobDateTime))
                    {
                        dateOfBirth = DateOnly.FromDateTime(parsedDobDateTime);
                    }
                    else if (double.TryParse(dobText, out var oaDate))
                    {
                        dateOfBirth = DateOnly.FromDateTime(DateTime.FromOADate(oaDate));
                    }
                }

                if (gender.Contains("أنثى") || gender.Equals("female", StringComparison.OrdinalIgnoreCase))
                {
                    gender = "Female";
                }
                else if (gender.Contains("ذكر") || gender.Equals("male", StringComparison.OrdinalIgnoreCase))
                {
                    gender = "Male";
                }
                else if (string.IsNullOrWhiteSpace(gender))
                {
                    gender = "Male";
                }

                models.Add(new CreatePassengerDto
                {
                    FullName = fullName,
                    PassportNumber = passportNumber,
                    PassportExpiry = passportExpiry,
                    NationalId = nationalId,
                    DateOfBirth = dateOfBirth,
                    Nationality = string.IsNullOrWhiteSpace(nationality) ? "Egyptian" : nationality,
                    Gender = gender,
                    PhoneNumber = phoneNumber,
                    Email = email,
                    Address = address,
                    EmergencyContactName = emergencyName,
                    EmergencyContactPhone = emergencyPhone,
                    PartnerId = partnerId
                });
            }

            var result = await _bookingService.ImportPassengersAsync(models);
            var message = $"Imported {result.ImportedCount} passengers. Duplicates skipped: {result.DuplicateCount}. Errors: {result.ImportErrors.Count}.";
            return Json(new { success = true, message, result.ImportedCount, result.DuplicateCount, result.ImportErrors });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error importing passengers from Excel.");
            return Json(new { success = false, message = "An error occurred while importing passengers from Excel." });
        }
    }

    /// <summary>
    /// Imports passengers from Excel and returns the imported passenger list as JSON
    /// for dynamic addition to the Create Booking page dropdown (without page reload).
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ImportPassengersExcel(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return Json(new { success = false, message = "يرجى رفع ملف Excel صالح." });

        using (var stream = file.OpenReadStream())
        {
            var validation = BusinessManagement.Shared.Security.FileSecurityValidator.ValidateFile(
                stream, file.FileName, file.ContentType, file.Length);
            if (!validation.IsValid)
                return Json(new { success = false, message = validation.ErrorMessage });
        }

        try
        {
            using var memoryStream = new MemoryStream();
            await file.CopyToAsync(memoryStream);
            memoryStream.Position = 0;
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using var package = new ExcelPackage(memoryStream);
            var worksheet = package.Workbook.Worksheets.FirstOrDefault();
            if (worksheet == null)
                return Json(new { success = false, message = "ملف Excel لا يحتوي على ورقة عمل صالحة." });

            var partnersLookup = await _bookingService.GetPartnersLookupAsync();
            var rows = worksheet.Dimension?.Rows ?? 0;
            var models = new List<CreatePassengerDto>();

            for (int row = 2; row <= rows; row++)
            {
                var fullName = worksheet.Cells[row, 1].Text?.Trim() ?? string.Empty;
                var partnerStr = worksheet.Cells[row, 2].Text?.Trim() ?? string.Empty;
                var passportNumber = worksheet.Cells[row, 3].Text?.Trim() ?? string.Empty;

                if (string.IsNullOrWhiteSpace(fullName) && 
                    string.IsNullOrWhiteSpace(partnerStr) && 
                    string.IsNullOrWhiteSpace(passportNumber))
                {
                    continue;
                }

                if (string.IsNullOrWhiteSpace(fullName))
                {
                    return Json(new { success = false, message = $"الصف {row}: اسم المسافر مطلوب ولا يمكن أن يكون فارغاً." });
                }

                Guid? partnerId = null;
                if (!string.IsNullOrWhiteSpace(partnerStr))
                {
                    var normPartnerStr = partnerStr.Trim().ToLower();
                    if (normPartnerStr == "mohammed") normPartnerStr = "mohamed";

                    var matchedPartner = partnersLookup.FirstOrDefault(pt => 
                        pt.PartnerName.Trim().Equals(normPartnerStr, StringComparison.OrdinalIgnoreCase) ||
                        pt.PartnerName.Trim().Equals(partnerStr, StringComparison.OrdinalIgnoreCase)
                    );

                    if (matchedPartner != null)
                    {
                        partnerId = matchedPartner.PartnerId;
                    }
                    else
                    {
                        return Json(new { success = false, message = $"الصف {row}: التبعية لـ ({partnerStr}) غير صالحة. يجب أن تكون أحد الشركاء: omar - khaled - mohammed - alaa." });
                    }
                }
                else
                {
                    return Json(new { success = false, message = $"الصف {row}: حقل التبعية مطلوب للمسافر ({fullName})." });
                }

                if (string.IsNullOrWhiteSpace(passportNumber) || !System.Text.RegularExpressions.Regex.IsMatch(passportNumber, "^[aA]\\d{7}$"))
                {
                    return Json(new { success = false, message = $"الصف {row}: رقم جواز السفر غير صالح لـ ({fullName}). يجب أن يبدأ بحرف A ويليه 7 أرقام (مثال: A1234567)." });
                }

                var passportExpiryCell = worksheet.Cells[row, 4];
                var nationalId = worksheet.Cells[row, 5].Text?.Trim() ?? string.Empty;
                var dateOfBirthCell = worksheet.Cells[row, 6];
                var nationality = worksheet.Cells[row, 7].Text?.Trim() ?? string.Empty;
                var gender = worksheet.Cells[row, 8].Text?.Trim() ?? string.Empty;
                var phoneNumber = worksheet.Cells[row, 9].Text?.Trim() ?? string.Empty;
                var email = worksheet.Cells[row, 10].Text?.Trim() ?? string.Empty;
                var address = worksheet.Cells[row, 11].Text?.Trim() ?? string.Empty;
                var emergencyName = worksheet.Cells[row, 12].Text?.Trim() ?? string.Empty;
                var emergencyPhone = worksheet.Cells[row, 13].Text?.Trim() ?? string.Empty;

                if (!string.IsNullOrWhiteSpace(nationalId) && !System.Text.RegularExpressions.Regex.IsMatch(nationalId, "^\\d{14}$"))
                {
                    return Json(new { success = false, message = $"الصف {row}: الرقم القومي لـ ({fullName}) غير صالح. يجب أن يتكون من 14 رقماً." });
                }

                DateOnly? passportExpiry = null;
                if (passportExpiryCell.Value != null)
                {
                    var expiryText = passportExpiryCell.Text?.Trim();
                    if (passportExpiryCell.Value is DateTime cellDateTime)
                    {
                        passportExpiry = DateOnly.FromDateTime(cellDateTime);
                    }
                    else if (DateOnly.TryParse(expiryText, out var parsedExpiry))
                    {
                        passportExpiry = parsedExpiry;
                    }
                    else if (DateTime.TryParse(expiryText, out var parsedDateTime))
                    {
                        passportExpiry = DateOnly.FromDateTime(parsedDateTime);
                    }
                    else if (double.TryParse(expiryText, out var oaDate))
                    {
                        passportExpiry = DateOnly.FromDateTime(DateTime.FromOADate(oaDate));
                    }
                }

                DateOnly? dateOfBirth = null;
                if (dateOfBirthCell.Value != null)
                {
                    var dobText = dateOfBirthCell.Text?.Trim();
                    if (dateOfBirthCell.Value is DateTime cellDateTime)
                    {
                        dateOfBirth = DateOnly.FromDateTime(cellDateTime);
                    }
                    else if (DateOnly.TryParse(dobText, out var parsedDob))
                    {
                        dateOfBirth = parsedDob;
                    }
                    else if (DateTime.TryParse(dobText, out var parsedDobDateTime))
                    {
                        dateOfBirth = DateOnly.FromDateTime(parsedDobDateTime);
                    }
                    else if (double.TryParse(dobText, out var oaDate))
                    {
                        dateOfBirth = DateOnly.FromDateTime(DateTime.FromOADate(oaDate));
                    }
                }

                if (gender.Contains("أنثى") || gender.Equals("female", StringComparison.OrdinalIgnoreCase))
                {
                    gender = "Female";
                }
                else if (gender.Contains("ذكر") || gender.Equals("male", StringComparison.OrdinalIgnoreCase))
                {
                    gender = "Male";
                }
                else if (string.IsNullOrWhiteSpace(gender))
                {
                    gender = "Male";
                }

                models.Add(new CreatePassengerDto
                {
                    FullName = fullName,
                    PassportNumber = passportNumber,
                    PassportExpiry = passportExpiry,
                    NationalId = nationalId,
                    DateOfBirth = dateOfBirth,
                    Nationality = string.IsNullOrWhiteSpace(nationality) ? "Egyptian" : nationality,
                    Gender = gender,
                    PhoneNumber = phoneNumber,
                    Email = email,
                    Address = address,
                    EmergencyContactName = emergencyName,
                    EmergencyContactPhone = emergencyPhone,
                    PartnerId = partnerId
                });
            }

            if (models.Count == 0)
                return Json(new { success = false, message = "لم يتم العثور على بيانات مسافرين في الملف. تأكد من أن البيانات تبدأ من الصف الثاني." });

            // Import & save to DB
            var importResult = await _bookingService.ImportPassengersAsync(models);

            // Fetch the newly created passengers to return their IDs
            var allPassengers = await _bookingService.GetAllPassengersAsync();
            var importedNames  = new HashSet<string>(models.Select(m => m.FullName.Trim()), StringComparer.OrdinalIgnoreCase);
            var importedList   = allPassengers
                .Where(p => importedNames.Contains(p.FullName.Trim()))
                .Select(p => new { p.Id, p.FullName, p.PassportNumber, p.PartnerId })
                .ToList();

            return Json(new
            {
                success    = true,
                message    = $"تم استيراد {importResult.ImportedCount} مسافر بنجاح. مكرر ومُتجاهَل: {importResult.DuplicateCount}.",
                passengers = importedList
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error importing passengers from Excel for booking form.");
            return Json(new { success = false, message = "حدث خطأ أثناء معالجة ملف Excel." });
        }
    }

    #endregion

    #region Booking Payments AJAX Handles
    [HttpGet]
    public async Task<IActionResult> Payments(Guid bookingId)
    {
        var booking = await _bookingService.GetBookingByIdAsync(bookingId);
        if (booking == null)
        {
            return NotFound("Booking not found.");
        }

        var payments = await _bookingService.GetBookingPaymentsAsync(bookingId);
        ViewBag.Booking = booking;
        return PartialView("_PaymentsModal", payments);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddPayment(
        [FromForm] Guid bookingId, 
        [FromForm] decimal amount, 
        [FromForm] string paymentMethod, 
        [FromForm] string receiptNumber, 
        [FromForm] string paymentDate,
        [FromForm] string? accountDetail)
    {
        if (amount <= 0)
        {
            return Json(new { success = false, message = "Amount must be greater than zero." });
        }

        try
        {
            var pDate = DateOnly.Parse(paymentDate);
            string mergedReceipt = receiptNumber ?? string.Empty;
            if (!string.IsNullOrWhiteSpace(accountDetail))
            {
                string label = paymentMethod == "BankTransfer" ? "حساب" : paymentMethod == "VodafoneCash" ? "محفظة" : paymentMethod == "InstaPay" ? "إنستا باي" : "مرجع";
                if (!string.IsNullOrWhiteSpace(mergedReceipt))
                {
                    mergedReceipt += $" ({label}: {accountDetail})";
                }
                else
                {
                    mergedReceipt = $"({label}: {accountDetail})";
                }
            }

            await _bookingService.AddBookingPaymentAsync(bookingId, amount, paymentMethod, mergedReceipt, pDate);
            return Json(new { success = true, message = "Payment recorded successfully!" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error recording booking payment.");
            return Json(new { success = false, message = ex.Message });
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeletePayment(Guid paymentId)
    {
        try
        {
            await _bookingService.DeleteBookingPaymentAsync(paymentId);
            return Json(new { success = true, message = "Payment deleted/reverted successfully!" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting booking payment.");
            return Json(new { success = false, message = ex.Message });
        }
    }
    #endregion

    #region Export and Printing
    [HttpGet]
    public async Task<IActionResult> ExportExcel(string? partner)
    {
        string? partnerFilter = User.IsInRole("Partner") ? User.Identity?.Name : partner;
        var bookings = await _bookingService.GetAllBookingsAsync(partnerFilter);

        var headers = new List<string>
        {
            "رقم الحجز", "المجموعة", "اسم المسافر", "جواز السفر", "الشريك التابع", "نت المنصة", "الباركود",
            "الشركة", "المطار", "البرنامج", "تذكرة الطيران", "الباص", "إجمالي التكلفة", "سعر البيع",
            "صافي الأرباح", "المدفوع", "المتبقي", "بوابة QR"
        };

        var rows = new List<List<object?>>();
        foreach (var b in bookings)
        {
            rows.Add(new List<object?>
            {
                b.BookingNumber,
                b.GroupNumber,
                b.PassengerName,
                b.PassengerPassport,
                b.PartnerName,
                b.NetCost,
                b.BarcodeCost,
                b.CompanyCost,
                b.AirportCost,
                b.ProgramCost,
                b.TicketCost,
                b.BusCost,
                b.TotalCost,
                b.SellingPrice,
                b.NetProfit,
                b.PaymentsCollected,
                b.RemainingBalance,
                b.HasQrCode ? "نعم" : "لا"
            });
        }

        var rightAlignCols = new HashSet<int> { 3, 5 };
        var textFormatCols = new HashSet<int> { 1, 2, 4 };

        byte[] fileContents = BusinessManagement.Web.Helpers.ExcelExportHelper.ExportToExcel(
            "حجوزات المسافرين",
            headers,
            rows,
            rightAlignCols,
            textFormatCols
        );

        return File(fileContents, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"حجوزات_المسافرين-{DateTime.Now:yyyyMMddHHmmss}.xlsx");
    }

    [HttpPost]
    public IActionResult ExportCustomExcel(
        string sheetName,
        string headersJson,
        string rowsJson,
        string? rightAlignColsJson,
        string? textFormatColsJson)
    {
        var headers = System.Text.Json.JsonSerializer.Deserialize<List<string>>(headersJson) ?? new List<string>();
        var rows = System.Text.Json.JsonSerializer.Deserialize<List<List<object?>>>(rowsJson) ?? new List<List<object?>>();
        var rightAlignCols = string.IsNullOrEmpty(rightAlignColsJson) ? null : System.Text.Json.JsonSerializer.Deserialize<HashSet<int>>(rightAlignColsJson);
        var textFormatCols = string.IsNullOrEmpty(textFormatColsJson) ? null : System.Text.Json.JsonSerializer.Deserialize<HashSet<int>>(textFormatColsJson);

        byte[] fileContents = BusinessManagement.Web.Helpers.ExcelExportHelper.ExportToExcel(
            sheetName,
            headers,
            rows,
            rightAlignCols,
            textFormatCols
        );

        return File(fileContents, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"{sheetName}-{DateTime.Now:yyyyMMddHHmmss}.xlsx");
    }

    [HttpGet]
    public async Task<IActionResult> Print(Guid id)
    {
        var booking = await _bookingService.GetBookingByIdAsync(id);
        if (booking == null) return NotFound("الحجز غير موجود.");

        var payments = await _bookingService.GetBookingPaymentsAsync(id);
        ViewBag.Payments = payments;
        return View(booking);
    }

    [HttpGet]
    public async Task<IActionResult> PrintBulk(string ids)
    {
        if (string.IsNullOrEmpty(ids))
        {
            return BadRequest("لم يتم تحديد أي ملفات للطباعة.");
        }

        var idList = ids.Split(',', StringSplitOptions.RemoveEmptyEntries)
                        .Select(x => Guid.TryParse(x, out var guid) ? guid : Guid.Empty)
                        .Where(x => x != Guid.Empty)
                        .ToList();

        var bookingsList = new List<BusinessManagement.Application.DTOs.BookingDto>();
        var paymentsMap = new Dictionary<Guid, IEnumerable<BusinessManagement.Application.DTOs.BookingPaymentDto>>();

        foreach (var id in idList)
        {
            var booking = await _bookingService.GetBookingByIdAsync(id);
            if (booking != null)
            {
                bookingsList.Add(booking);
                var payments = await _bookingService.GetBookingPaymentsAsync(id);
                paymentsMap[id] = payments;
            }
        }

        ViewBag.PaymentsMap = paymentsMap;
        return View(bookingsList);
    }
    #endregion
}





