using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using BusinessManagement.Application.DTOs;
using BusinessManagement.Application.Interfaces;
using BusinessManagement.Web.Filters;

namespace BusinessManagement.Web.Controllers;

[Authorize]
public class CustomersController : Controller
{
    private readonly ICustomerService _customerService;
    private readonly ISecurityAuditService _auditService;
    private readonly IBookingService _bookingService;
    private readonly ILogger<CustomersController> _logger;

    public CustomersController(
        ICustomerService customerService,
        ISecurityAuditService auditService,
        IBookingService bookingService,
        ILogger<CustomersController> logger)
    {
        _customerService = customerService;
        _auditService = auditService;
        _bookingService = bookingService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> Index(
        string? search,
        Guid? partnerId,
        string? gender,
        string? nationality,
        string? groupNumber,
        string? sortBy = "createdat",
        string? sortOrder = "desc",
        int page = 1,
        int pageSize = 10)
    {
        var result = await _customerService.GetCustomersPagedAsync(search, gender, nationality, sortBy, sortOrder, page, pageSize, partnerId, groupNumber);

        var partners = await _bookingService.GetPartnersLookupAsync();
        ViewBag.Partners = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(partners, "PartnerId", "PartnerName", partnerId);

        // Fetch all unique group numbers to populate the dropdown filter
        var allPassengers = await _customerService.GetCustomersPagedAsync(null, null, null, "name", "asc", 1, 1000000, null);
        var groupNumbers = allPassengers.Customers
            .Select(c => c.GroupNumber)
            .Where(g => !string.IsNullOrWhiteSpace(g))
            .Distinct()
            .OrderBy(g => g)
            .ToList();
        ViewBag.GroupNumbers = groupNumbers;

        ViewBag.Search = search;
        ViewBag.PartnerId = partnerId;
        ViewBag.Gender = gender;
        ViewBag.Nationality = nationality;
        ViewBag.GroupNumber = groupNumber;
        ViewBag.SortBy = sortBy;
        ViewBag.SortOrder = sortOrder;
        ViewBag.SuccessMessage = TempData["SuccessMessage"];
        ViewBag.ErrorMessage = TempData["ErrorMessage"];

        return View(result);
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        var partners = await _bookingService.GetPartnersLookupAsync();
        ViewBag.Partners = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(partners, "PartnerId", "PartnerName");
        return View(new CustomerDetailsDto());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CustomerDetailsDto model)
    {
        if (!ModelState.IsValid)
        {
            var partners = await _bookingService.GetPartnersLookupAsync();
            ViewBag.Partners = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(partners, "PartnerId", "PartnerName", model.PartnerId);
            return View(model);
        }

        try
        {
            var created = await _customerService.CreateCustomerAsync(model);
            TempData["SuccessMessage"] = "تم تسجيل العميل بنجاح!";
            
            string userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "system";
            Guid.TryParse(userId, out var guidUserId);
            await _auditService.LogActivityAsync(guidUserId, $"Create Customer: {model.FullName}", HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown", Request.Headers["User-Agent"].ToString());

            return RedirectToAction("Details", new { id = created.Id });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating customer: {Name}", model.FullName);
            ModelState.AddModelError(string.Empty, "حدث خطأ أثناء حفظ بيانات العميل. يرجى التحقق من المدخلات.");
            var partners = await _bookingService.GetPartnersLookupAsync();
            ViewBag.Partners = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(partners, "PartnerId", "PartnerName", model.PartnerId);
            return View(model);
        }
    }

    [HttpGet]
    public async Task<IActionResult> Edit(Guid id)
    {
        var customer = await _customerService.GetCustomerByIdAsync(id);
        if (customer == null) return NotFound("العميل غير موجود.");

        var partners = await _bookingService.GetPartnersLookupAsync();
        ViewBag.Partners = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(partners, "PartnerId", "PartnerName", customer.PartnerId);

        return View(customer);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(CustomerDetailsDto model)
    {
        if (!ModelState.IsValid)
        {
            var partners = await _bookingService.GetPartnersLookupAsync();
            ViewBag.Partners = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(partners, "PartnerId", "PartnerName", model.PartnerId);
            return View(model);
        }

        try
        {
            bool success = await _customerService.UpdateCustomerAsync(model);
            if (success)
            {
                TempData["SuccessMessage"] = "تم تحديث بيانات العميل بنجاح!";
                
                string userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "system";
                Guid.TryParse(userId, out var guidUserId);
                await _auditService.LogActivityAsync(guidUserId, $"Update Customer: {model.FullName}", HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown", Request.Headers["User-Agent"].ToString());

                return RedirectToAction("Details", new { id = model.Id });
            }

            ModelState.AddModelError(string.Empty, "فشل تحديث بيانات العميل.");
            var partners = await _bookingService.GetPartnersLookupAsync();
            ViewBag.Partners = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(partners, "PartnerId", "PartnerName", model.PartnerId);
            return View(model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error editing customer ID: {CustomerId}", model.Id);
            ModelState.AddModelError(string.Empty, "حدث خطأ غير متوقع أثناء حفظ البيانات.");
            var partners = await _bookingService.GetPartnersLookupAsync();
            ViewBag.Partners = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(partners, "PartnerId", "PartnerName", model.PartnerId);
            return View(model);
        }
    }

    [HttpGet]
    public async Task<IActionResult> Details(Guid id)
    {
        var customer = await _customerService.GetCustomerByIdAsync(id);
        if (customer == null) return NotFound("العميل غير موجود.");

        ViewBag.Notes = await _customerService.GetNotesForCustomerAsync(id);
        ViewBag.Attachments = await _customerService.GetAttachmentsForCustomerAsync(id);
        ViewBag.History = await _customerService.GetCustomerBookingHistoryAsync(id);
        ViewBag.SuccessMessage = TempData["SuccessMessage"];
        ViewBag.ErrorMessage = TempData["ErrorMessage"];

        return View(customer);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(Guid id)
    {
        try
        {
            bool success = await _customerService.DeleteCustomerAsync(id);
            if (success)
            {
                TempData["SuccessMessage"] = "تم حذف العميل بنجاح.";
            }
            else
            {
                TempData["ErrorMessage"] = "فشل حذف العميل.";
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting customer ID: {CustomerId}", id);
            TempData["ErrorMessage"] = "لا يمكن حذف هذا العميل لوجود حجوزات سابقة مرتبطة بملفه.";
        }

        return RedirectToAction("Index");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddNote(Guid customerId, string noteText)
    {
        if (string.IsNullOrWhiteSpace(noteText))
        {
            TempData["ErrorMessage"] = "لا يمكن إضافة ملاحظة فارغة.";
            return RedirectToAction("Details", new { id = customerId });
        }

        string author = User.Identity?.Name ?? "system";
        await _customerService.AddNoteToCustomerAsync(customerId, noteText.Trim(), author);
        TempData["SuccessMessage"] = "تمت إضافة الملاحظة بنجاح.";

        return RedirectToAction("Details", new { id = customerId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteNote(Guid noteId, Guid customerId)
    {
        await _customerService.DeleteNoteAsync(noteId);
        TempData["SuccessMessage"] = "تم حذف الملاحظة.";
        return RedirectToAction("Details", new { id = customerId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UploadAttachment(Guid customerId, IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            TempData["ErrorMessage"] = "الملف المرفوع غير صالح.";
            return RedirectToAction("Details", new { id = customerId });
        }

        // Validate file security (whitelist, magic number, zip bomb, antivirus)
        using (var stream = file.OpenReadStream())
        {
            var validation = BusinessManagement.Shared.Security.FileSecurityValidator.ValidateFile(stream, file.FileName, file.ContentType, file.Length);
            if (!validation.IsValid)
            {
                TempData["ErrorMessage"] = validation.ErrorMessage;
                return RedirectToAction("Details", new { id = customerId });
            }
        }

        // Strip image metadata if image (Point 6/v2 & Point 5/v4)
        byte[] fileBytes;
        using (var memoryStream = new MemoryStream())
        {
            await file.CopyToAsync(memoryStream);
            fileBytes = memoryStream.ToArray();
        }

        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        fileBytes = BusinessManagement.Shared.Security.FileSecurityValidator.StripImageMetadata(fileBytes, ext);

        // Save file securely to uploads folder
        var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "customers");
        if (!Directory.Exists(uploadsFolder))
        {
            Directory.CreateDirectory(uploadsFolder);
        }

        var uniqueFileName = $"{Guid.NewGuid().ToString("N").Substring(0, 8)}_{Path.GetFileName(file.FileName)}";
        var filePath = Path.Combine(uploadsFolder, uniqueFileName);

        await System.IO.File.WriteAllBytesAsync(filePath, fileBytes);

        var dbPath = $"/uploads/customers/{uniqueFileName}";
        await _customerService.AddAttachmentToCustomerAsync(customerId, file.FileName, dbPath, file.ContentType, file.Length);
        TempData["SuccessMessage"] = "تم رفع المرفق بنجاح بعد الفحص والتحقق الأمني!";

        return RedirectToAction("Details", new { id = customerId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteAttachment(Guid attachmentId, Guid customerId)
    {
        await _customerService.DeleteAttachmentAsync(attachmentId);
        TempData["SuccessMessage"] = "تم حذف المرفق.";
        return RedirectToAction("Details", new { id = customerId });
    }

    [HttpGet]
    public async Task<IActionResult> ExportExcel(string? search, Guid? partnerId, string? gender, string? nationality, string? groupNumber)
    {
        var result = await _customerService.GetCustomersPagedAsync(search, gender, nationality, "name", "asc", 1, 10000, partnerId, groupNumber);

        var headers = new List<string>
        {
            "رقم المجموعة", "الاسم بالكامل", "التبعية (الشريك)", "رقم جواز السفر", "الرقم القومي", "الجنسية", "الجنس", "رقم الهاتف", "البريد الإلكتروني", "العنوان"
        };

        var rows = new List<List<object?>>();
        foreach (var c in result.Customers)
        {
            rows.Add(new List<object?>
            {
                string.IsNullOrWhiteSpace(c.GroupNumber) ? "-" : c.GroupNumber,
                c.FullName,
                c.PartnerName ?? "عميل مباشر",
                c.PassportNumber,
                c.NationalId,
                c.Nationality,
                c.Gender,
                c.PhoneNumber,
                c.Email,
                c.Address
            });
        }

        var rightAlignCols = new HashSet<int> { 2, 10 };
        var textFormatCols = new HashSet<int> { 1, 4, 5, 8 };

        byte[] fileContents = BusinessManagement.Web.Helpers.ExcelExportHelper.ExportToExcel(
            "قائمة العملاء",
            headers,
            rows,
            rightAlignCols,
            textFormatCols
        );

        return File(fileContents, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"العملاء-{DateTime.Now:yyyyMMddHHmmss}.xlsx");
    }

    [HttpGet]
    public async Task<IActionResult> Print(Guid id)
    {
        var customer = await _customerService.GetCustomerByIdAsync(id);
        if (customer == null) return NotFound();

        ViewBag.History = await _customerService.GetCustomerBookingHistoryAsync(id);
        return View(customer);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ImportExcel(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return Json(new { success = false, message = "يرجى تحميل ملف إكسيل صالح." });
        }

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

            OfficeOpenXml.ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;
            using var package = new OfficeOpenXml.ExcelPackage(memoryStream);
            var worksheet = package.Workbook.Worksheets.FirstOrDefault();
            if (worksheet == null)
            {
                return Json(new { success = false, message = "ملف إكسيل غير صالح ولا يحتوي على أوراق عمل." });
            }

            var rows = worksheet.Dimension?.Rows ?? 0;
            var dtos = new List<CustomerDetailsDto>();

            for (int row = 2; row <= rows; row++)
            {
                var fullName = worksheet.Cells[row, 1].Text?.Trim() ?? string.Empty;
                var affiliation = worksheet.Cells[row, 2].Text?.Trim() ?? string.Empty;
                var passportNumber = worksheet.Cells[row, 3].Text?.Trim() ?? string.Empty;
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

                if (string.IsNullOrWhiteSpace(fullName) && 
                    string.IsNullOrWhiteSpace(passportNumber) && 
                    passportExpiryCell.Value == null && 
                    string.IsNullOrWhiteSpace(nationalId) && 
                    dateOfBirthCell.Value == null && 
                    string.IsNullOrWhiteSpace(nationality) && 
                    string.IsNullOrWhiteSpace(gender) && 
                    string.IsNullOrWhiteSpace(phoneNumber) &&
                    string.IsNullOrWhiteSpace(affiliation))
                {
                    continue;
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

                dtos.Add(new CustomerDetailsDto
                {
                    FullName = fullName,
                    PassportNumber = passportNumber,
                    PassportExpiry = passportExpiry,
                    NationalId = nationalId,
                    DateOfBirth = dateOfBirth,
                    Nationality = string.IsNullOrWhiteSpace(nationality) ? "Egyptian" : nationality,
                    Gender = string.IsNullOrWhiteSpace(gender) ? "Male" : gender,
                    PhoneNumber = phoneNumber,
                    Email = email,
                    Address = address,
                    EmergencyContactName = emergencyName,
                    EmergencyContactPhone = emergencyPhone,
                    PartnerName = affiliation
                });
            }

            var result = await _customerService.ImportCustomersFromExcelAsync(dtos);
            
            string userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "system";
            Guid.TryParse(userId, out var guidUserId);
            await _auditService.LogActivityAsync(guidUserId, $"Import Passengers Excel: Imported {result.ImportedCount}, Duplicates: {result.DuplicateCount}, Errors: {result.ImportErrors.Count}", HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown", Request.Headers["User-Agent"].ToString());

            var message = $"تم استيراد {result.ImportedCount} مسافر بنجاح. تم تخطي {result.DuplicateCount} مكررين. عدد الأخطاء: {result.ImportErrors.Count}.";
            return Json(new { success = true, message, result.ImportedCount, result.DuplicateCount, result.ImportErrors });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error importing customers from Excel.");
            return Json(new { success = false, message = "حدث خطأ أثناء معالجة واستيراد ملف الإكسيل." });
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ImportGroupExcel(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return Json(new { success = false, message = "يرجى تحميل ملف إكسيل صالح." });
        }

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

            OfficeOpenXml.ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;
            using var package = new OfficeOpenXml.ExcelPackage(memoryStream);
            var worksheet = package.Workbook.Worksheets.FirstOrDefault();
            if (worksheet == null)
            {
                return Json(new { success = false, message = "ملف إكسيل غير صالح ولا يحتوي على أوراق عمل." });
            }

            var rows = worksheet.Dimension?.Rows ?? 0;
            var dtos = new List<CustomerDetailsDto>();

            for (int row = 2; row <= rows; row++)
            {
                var fullName = worksheet.Cells[row, 1].Text?.Trim() ?? string.Empty;
                var affiliation = worksheet.Cells[row, 2].Text?.Trim() ?? string.Empty;
                var passportNumber = worksheet.Cells[row, 3].Text?.Trim() ?? string.Empty;
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

                if (string.IsNullOrWhiteSpace(fullName) && 
                    string.IsNullOrWhiteSpace(passportNumber) && 
                    passportExpiryCell.Value == null && 
                    string.IsNullOrWhiteSpace(nationalId) && 
                    dateOfBirthCell.Value == null && 
                    string.IsNullOrWhiteSpace(nationality) && 
                    string.IsNullOrWhiteSpace(gender) && 
                    string.IsNullOrWhiteSpace(phoneNumber) &&
                    string.IsNullOrWhiteSpace(affiliation))
                {
                    continue;
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

                dtos.Add(new CustomerDetailsDto
                {
                    FullName = fullName,
                    PassportNumber = passportNumber,
                    PassportExpiry = passportExpiry,
                    NationalId = nationalId,
                    DateOfBirth = dateOfBirth,
                    Nationality = string.IsNullOrWhiteSpace(nationality) ? "Egyptian" : nationality,
                    Gender = string.IsNullOrWhiteSpace(gender) ? "Male" : gender,
                    PhoneNumber = phoneNumber,
                    Email = email,
                    Address = address,
                    EmergencyContactName = emergencyName,
                    EmergencyContactPhone = emergencyPhone,
                    PartnerName = affiliation
                });
            }

            var groupNum = "GRP-" + DateTime.UtcNow.ToString("yyyyMMdd") + "-" + new Random().Next(1000, 9999);
            var result = await _customerService.ImportCustomersFromExcelAsync(dtos, groupNum);
            
            string userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "system";
            Guid.TryParse(userId, out var guidUserId);
            await _auditService.LogActivityAsync(guidUserId, $"Import Passenger Group Excel: Group {groupNum}, Imported {result.ImportedCount}, Duplicates: {result.DuplicateCount}, Errors: {result.ImportErrors.Count}", HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown", Request.Headers["User-Agent"].ToString());

            var message = $"تم استيراد {result.ImportedCount} مسافر للمجموعة {groupNum} بنجاح. تم تخطي {result.DuplicateCount} مكررين. عدد الأخطاء: {result.ImportErrors.Count}.";
            return Json(new { success = true, message, result.ImportedCount, result.DuplicateCount, result.ImportErrors });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error importing customers group from Excel.");
            return Json(new { success = false, message = "حدث خطأ أثناء معالجة واستيراد ملف إكسيل للمجموعة." });
        }
    }

    [HttpGet]
    public async Task<IActionResult> CreateGroup()
    {
        ViewData["PageHeader"] = "إضافة مجموعة مسافرين جديدة";
        var partners = await _bookingService.GetPartnersLookupAsync();
        ViewBag.Partners = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(partners, "PartnerId", "PartnerName");
        
        var defaultGroupNum = "GRP-" + DateTime.UtcNow.ToString("yyyyMMdd") + "-" + new Random().Next(1000, 9999);
        ViewBag.DefaultGroupNumber = defaultGroupNum;

        return View(new List<CustomerDetailsDto>());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateGroup(string groupNumber, List<CustomerDetailsDto> passengers)
    {
        if (string.IsNullOrWhiteSpace(groupNumber))
        {
            ModelState.AddModelError("", "رقم المجموعة مطلوب.");
        }
        if (passengers == null || !passengers.Any(p => !string.IsNullOrWhiteSpace(p.FullName)))
        {
            ModelState.AddModelError("", "يجب إضافة مسافر واحد على الأقل.");
        }

        if (ModelState.IsValid && passengers != null)
        {
            int createdCount = 0;
            foreach (var passenger in passengers)
            {
                if (string.IsNullOrWhiteSpace(passenger.FullName) || string.IsNullOrWhiteSpace(passenger.PassportNumber))
                {
                    continue;
                }
                
                passenger.GroupNumber = groupNumber;
                await _customerService.CreateCustomerAsync(passenger);
                createdCount++;
            }

            TempData["SuccessMessage"] = $"تم إضافة مجموعة المسافرين ({groupNumber}) بنجاح. العدد: {createdCount} مسافر.";
            return RedirectToAction("Index");
        }

        var partners = await _bookingService.GetPartnersLookupAsync();
        ViewBag.Partners = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(partners, "PartnerId", "PartnerName");
        ViewBag.DefaultGroupNumber = groupNumber;

        return View(passengers);
    }
}
