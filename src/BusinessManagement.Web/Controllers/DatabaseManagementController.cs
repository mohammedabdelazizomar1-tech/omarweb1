using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using BusinessManagement.Application.Interfaces;
using BusinessManagement.Domain.Entities;
using BusinessManagement.Domain.Repositories;

namespace BusinessManagement.Web.Controllers;

[Authorize(Roles = "Admin")]
public class DatabaseManagementController : Controller
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IBookingService _bookingService;
    private readonly ILogger<DatabaseManagementController> _logger;

    public DatabaseManagementController(
        IUnitOfWork unitOfWork,
        IBookingService bookingService,
        ILogger<DatabaseManagementController> logger)
    {
        _unitOfWork = unitOfWork;
        _bookingService = bookingService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        ViewData["PageHeader"] = "لوحة تحكم وإدارة قاعدة البيانات";
        
        // Fetch database stats for the admin view
        ViewBag.BookingsCount = (await _unitOfWork.GetRepository<Booking>().GetAllAsync()).Count();
        ViewBag.PassengersCount = (await _unitOfWork.GetRepository<Passenger>().GetAllAsync()).Count();
        ViewBag.VisasCount = (await _unitOfWork.GetRepository<ExternalVisa>().GetAllAsync()).Count();
        ViewBag.SafeTransactionsCount = (await _unitOfWork.GetRepository<SafeTransaction>().GetAllAsync()).Count();
        ViewBag.JournalEntriesCount = (await _unitOfWork.GetRepository<JournalEntry>().GetAllAsync()).Count();
        ViewBag.CapitalCount = (await _unitOfWork.GetRepository<PartnershipCapital>().GetAllAsync()).Count();

        ViewBag.SuccessMessage = TempData["SuccessMessage"];
        ViewBag.ErrorMessage = TempData["ErrorMessage"];

        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ClearAllData()
    {
        _logger.LogWarning("Admin User {User} requested clearing ALL operational database tables.", User.Identity?.Name);
        try
        {
            await ClearBookingsInternalAsync();
            await ClearFinancialsInternalAsync();
            await ClearPassengersInternalAsync();
            await ClearVisasInternalAsync();
            await ClearCapitalInternalAsync();

            await _unitOfWork.CompleteAsync();
            TempData["SuccessMessage"] = "تم تصفية وحذف كافة البيانات التشغيلية والمالية بنجاح، وإرجاع النظام للحالة الصفرية للتشغيل!";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred during clear all operational data request.");
            TempData["ErrorMessage"] = $"فشلت عملية مسح البيانات: {ex.Message}";
        }

        return RedirectToAction("Index");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ClearBookings()
    {
        _logger.LogWarning("Admin User {User} requested clearing Bookings and payments tables.", User.Identity?.Name);
        try
        {
            await ClearBookingsInternalAsync();
            await _unitOfWork.CompleteAsync();
            TempData["SuccessMessage"] = "تم حذف كافة حجوزات المسافرين وتكاليفها ودفعاتها من الخادم بنجاح!";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error clearing bookings.");
            TempData["ErrorMessage"] = $"فشل مسح الحجوزات: {ex.Message}";
        }
        return RedirectToAction("Index");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ClearFinancials()
    {
        _logger.LogWarning("Admin User {User} requested clearing Ledger entries and Safe logs.", User.Identity?.Name);
        try
        {
            await ClearFinancialsInternalAsync();
            await _unitOfWork.CompleteAsync();
            TempData["SuccessMessage"] = "تم تصفية القيود المحاسبية وحركة الخزينة والواردات والمصروفات بنجاح!";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error clearing financials.");
            TempData["ErrorMessage"] = $"فشل مسح القيود المالية: {ex.Message}";
        }
        return RedirectToAction("Index");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ClearPassengers()
    {
        _logger.LogWarning("Admin User {User} requested clearing Passengers and passenger notes tables.", User.Identity?.Name);
        try
        {
            var bookingsCount = (await _unitOfWork.GetRepository<Booking>().GetAllAsync()).Count();
            if (bookingsCount > 0)
            {
                TempData["ErrorMessage"] = "لا يمكن مسح المسافرين لوجود حجوزات مرتبطة بهم. يرجى مسح الحجوزات أولاً.";
                return RedirectToAction("Index");
            }

            await ClearPassengersInternalAsync();
            await _unitOfWork.CompleteAsync();
            TempData["SuccessMessage"] = "تم تصفية وحذف كافة المسافرين وملاحظاتهم بنجاح!";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error clearing passengers.");
            TempData["ErrorMessage"] = $"فشل مسح المسافرين: {ex.Message}";
        }
        return RedirectToAction("Index");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ImportBusinessExcel(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            TempData["ErrorMessage"] = "يرجى اختيار ملف Excel صحيح لرفعه.";
            return RedirectToAction("Index");
        }

        // Validate file security
        using (var stream = file.OpenReadStream())
        {
            var validation = BusinessManagement.Shared.Security.FileSecurityValidator.ValidateFile(stream, file.FileName, file.ContentType, file.Length);
            if (!validation.IsValid)
            {
                TempData["ErrorMessage"] = $"تحذير أمني: {validation.ErrorMessage}";
                return RedirectToAction("Index");
            }
        }

        try
        {
            using var memoryStream = new MemoryStream();
            await file.CopyToAsync(memoryStream);
            memoryStream.Position = 0;

            var result = await _bookingService.ImportFullBusinessExcelAsync(memoryStream);
            
            TempData["SuccessMessage"] = $"تم اكتمال الاستيراد بنجاح! السجلات المضافة: " +
                $"{result.CapitalImportedCount} شريك/رأسمال، " +
                $"{result.BookingsImportedCount} حجز ومسافر، " +
                $"{result.ExternalVisasImportedCount} فيزا خارجية، " +
                $"{result.SafeTransactionsImportedCount} حركة خزينة.";

            return RedirectToAction("Index");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error importing business Excel file.");
            TempData["ErrorMessage"] = $"حدث خطأ أثناء فك ومعالجة البيانات: {ex.Message}";
            return RedirectToAction("Index");
        }
    }

    #region Helper Methods
    private async Task ClearBookingsInternalAsync()
    {
        var bookingRepo = _unitOfWork.GetRepository<Booking>();
        var paymentRepo = _unitOfWork.GetRepository<BookingPayment>();
        var costRepo = _unitOfWork.GetRepository<BookingCost>();
        var snapshotRepo = _unitOfWork.GetRepository<BookingSnapshot>();
        var historyRepo = _unitOfWork.GetRepository<BookingStatusHistory>();
        
        // 1. Delete all dependent child entities first
        await paymentRepo.ExecuteDeleteAsync(p => true, ignoreQueryFilters: true);
        await costRepo.ExecuteDeleteAsync(c => true, ignoreQueryFilters: true);
        await snapshotRepo.ExecuteDeleteAsync(s => true, ignoreQueryFilters: true);
        await historyRepo.ExecuteDeleteAsync(h => true, ignoreQueryFilters: true);

        // 2. Finally, delete the parent Bookings
        await bookingRepo.ExecuteDeleteAsync(b => true, ignoreQueryFilters: true);
    }

    private async Task ClearFinancialsInternalAsync()
    {
        var entryRepo = _unitOfWork.GetRepository<JournalEntry>();
        var lineRepo = _unitOfWork.GetRepository<JournalEntryLine>();
        var safeRepo = _unitOfWork.GetRepository<SafeTransaction>();
        var expRepo = _unitOfWork.GetRepository<Expense>();
        var revRepo = _unitOfWork.GetRepository<Revenue>();

        // 1. Delete dependent JournalEntryLines first
        await lineRepo.ExecuteDeleteAsync(l => true, ignoreQueryFilters: true);

        // 2. Delete parent JournalEntries
        await entryRepo.ExecuteDeleteAsync(e => true, ignoreQueryFilters: true);
        await safeRepo.ExecuteDeleteAsync(s => true, ignoreQueryFilters: true);
        await expRepo.ExecuteDeleteAsync(ex => true, ignoreQueryFilters: true);
        await revRepo.ExecuteDeleteAsync(r => true, ignoreQueryFilters: true);
    }

    private async Task ClearPassengersInternalAsync()
    {
        var passengerRepo = _unitOfWork.GetRepository<Passenger>();
        var noteRepo = _unitOfWork.GetRepository<PassengerNote>();

        // 1. Delete passenger notes first
        await noteRepo.ExecuteDeleteAsync(n => true, ignoreQueryFilters: true);

        // 2. Delete parent passengers
        await passengerRepo.ExecuteDeleteAsync(p => true, ignoreQueryFilters: true);
    }

    private async Task ClearVisasInternalAsync()
    {
        var visaRepo = _unitOfWork.GetRepository<ExternalVisa>();
        await visaRepo.ExecuteDeleteAsync(v => true, ignoreQueryFilters: true);
    }

    private async Task ClearCapitalInternalAsync()
    {
        var capitalRepo = _unitOfWork.GetRepository<PartnershipCapital>();
        await capitalRepo.ExecuteDeleteAsync(c => true, ignoreQueryFilters: true);
    }
    #endregion
}
