using System;
using System.IO;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using BusinessManagement.Application.DTOs;
using BusinessManagement.Application.Interfaces;

namespace BusinessManagement.Web.Controllers;

[Authorize]
public class ProfileController : Controller
{
    private readonly IIdentityService _identityService;
    private readonly ISessionService _sessionService;
    private readonly ISecurityAuditService _auditService;
    private readonly IUserService _userService;
    private readonly ILogger<ProfileController> _logger;

    public ProfileController(
        IIdentityService identityService,
        ISessionService sessionService,
        ISecurityAuditService auditService,
        IUserService userService,
        ILogger<ProfileController> logger)
    {
        _identityService = identityService;
        _sessionService = sessionService;
        _auditService = auditService;
        _userService = userService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var userId = GetCurrentUserId();
        var user = await _userService.GetUserByIdAsync(userId);
        if (user == null) return NotFound("المستخدم غير موجود.");

        ViewBag.SuccessMessage = TempData["SuccessMessage"];
        ViewBag.ErrorMessage = TempData["ErrorMessage"];

        return View(user);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Update(string firstName, string lastName, IFormFile? profilePicture)
    {
        var userId = GetCurrentUserId();
        string? profilePictureUrl = null;

        try
        {
            if (profilePicture != null && profilePicture.Length > 0)
            {
                // Validate file security
                using (var stream = profilePicture.OpenReadStream())
                {
                    var validation = BusinessManagement.Shared.Security.FileSecurityValidator.ValidateFile(stream, profilePicture.FileName, profilePicture.ContentType, profilePicture.Length);
                    if (!validation.IsValid)
                    {
                        TempData["ErrorMessage"] = validation.ErrorMessage;
                        return RedirectToAction("Index");
                    }
                }

                // Strip EXIF metadata from profile picture
                byte[] fileBytes;
                using (var memoryStream = new MemoryStream())
                {
                    await profilePicture.CopyToAsync(memoryStream);
                    fileBytes = memoryStream.ToArray();
                }

                var ext = Path.GetExtension(profilePicture.FileName).ToLowerInvariant();
                fileBytes = BusinessManagement.Shared.Security.FileSecurityValidator.StripImageMetadata(fileBytes, ext);

                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "profiles");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                var uniqueFileName = $"{userId}_{Guid.NewGuid().ToString("N").Substring(0, 8)}{ext}";
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                await System.IO.File.WriteAllBytesAsync(filePath, fileBytes);

                profilePictureUrl = $"/uploads/profiles/{uniqueFileName}";
            }

            bool success = await _identityService.UpdateProfileAsync(userId, firstName, lastName, profilePictureUrl);
            if (success)
            {
                TempData["SuccessMessage"] = "تم تحديث الملف الشخصي بنجاح!";
            }
            else
            {
                TempData["ErrorMessage"] = "فشل تحديث البيانات الشخصية.";
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating profile for user {User}", userId);
            TempData["ErrorMessage"] = "حدث خطأ أثناء حفظ التعديلات.";
        }

        return RedirectToAction("Index");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangePassword(string currentPassword, string newPassword, string confirmPassword)
    {
        var userId = GetCurrentUserId();

        if (newPassword != confirmPassword)
        {
            TempData["ErrorMessage"] = "كلمة المرور الجديدة وتأكيدها غير متطابقين.";
            return RedirectToAction("Index");
        }

        try
        {
            bool success = await _identityService.ChangePasswordAsync(userId, currentPassword, newPassword);
            if (success)
            {
                TempData["SuccessMessage"] = "تم تغيير كلمة المرور بنجاح!";
            }
            else
            {
                TempData["ErrorMessage"] = "كلمة المرور الحالية غير صحيحة.";
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error changing password for user {User}", userId);
            TempData["ErrorMessage"] = "حدث خطأ أثناء تغيير كلمة المرور.";
        }

        return RedirectToAction("Index");
    }

    [HttpGet]
    public async Task<IActionResult> Sessions()
    {
        var userId = GetCurrentUserId();
        var sessions = await _sessionService.GetActiveSessionsForUserAsync(userId);
        
        ViewBag.SuccessMessage = TempData["SuccessMessage"];
        ViewBag.ErrorMessage = TempData["ErrorMessage"];
        ViewBag.CurrentIp = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        ViewBag.CurrentUserAgent = Request.Headers["User-Agent"].ToString() ?? "unknown";

        return View(sessions);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RevokeSession(string token)
    {
        var userId = GetCurrentUserId();
        try
        {
            bool success = await _sessionService.RevokeSessionAsync(token, userId);
            if (success)
            {
                TempData["SuccessMessage"] = "تم إنهاء جلسة تسجيل الدخول المحددة بنجاح.";
            }
            else
            {
                TempData["ErrorMessage"] = "فشل إنهاء الجلسة.";
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error revoking session for user {User}", userId);
            TempData["ErrorMessage"] = "حدث خطأ أثناء إنهاء الجلسة.";
        }

        return RedirectToAction("Sessions");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RevokeAllOtherSessions()
    {
        var userId = GetCurrentUserId();
        var sessions = await _sessionService.GetActiveSessionsForUserAsync(userId);
        
        string ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        string userAgent = Request.Headers["User-Agent"].ToString() ?? "unknown";
        var currentSession = sessions.FirstOrDefault(s => s.IpAddress == ipAddress && s.UserAgent == userAgent);

        try
        {
            if (currentSession != null)
            {
                await _sessionService.RevokeAllSessionsForUserExceptCurrentAsync(userId, currentSession.Token);
                TempData["SuccessMessage"] = "تم تسجيل الخروج من جميع الأجهزة الأخرى بنجاح!";
            }
            else
            {
                await _sessionService.RevokeAllSessionsForUserAsync(userId);
                return RedirectToAction("Logout", "Account");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error revoking other sessions for user {User}", userId);
            TempData["ErrorMessage"] = "حدث خطأ أثناء تسجيل الخروج من الأجهزة الأخرى.";
        }

        return RedirectToAction("Sessions");
    }

    private Guid GetCurrentUserId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (claim == null || !Guid.TryParse(claim.Value, out var id))
        {
            throw new UnauthorizedAccessException("المستخدم غير مسجل الدخول.");
        }
        return id;
    }
}
