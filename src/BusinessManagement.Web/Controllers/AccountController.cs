using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using BusinessManagement.Application.DTOs;
using BusinessManagement.Application.Interfaces;

namespace BusinessManagement.Web.Controllers;

public class AccountController : Controller
{
    private readonly IIdentityService _identityService;
    private readonly ISessionService _sessionService;
    private readonly ISecurityAuditService _auditService;
    private readonly IUserService _userService;
    private readonly ILogger<AccountController> _logger;

    public AccountController(
        IIdentityService identityService,
        ISessionService sessionService,
        ISecurityAuditService auditService,
        IUserService userService,
        ILogger<AccountController> logger)
    {
        _identityService = identityService;
        _sessionService = sessionService;
        _auditService = auditService;
        _userService = userService;
        _logger = logger;
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToAction("Index", "Dashboard");
        }

        ViewData["ReturnUrl"] = returnUrl;
        return View(new LoginRequestDto());
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    [Microsoft.AspNetCore.RateLimiting.EnableRateLimiting("LoginPolicy")]
    public async Task<IActionResult> Login(LoginRequestDto model, string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            string ipAddress = HttpContext.Connection?.RemoteIpAddress?.ToString() ?? "unknown";
            string userAgent = Request.Headers != null && Request.Headers.ContainsKey("User-Agent") ? Request.Headers["User-Agent"].ToString() : "unknown";

            var user = await _identityService.AuthenticateAsync(model.UsernameOrEmail, model.Password, ipAddress, userAgent);
            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "اسم المستخدم/البريد الإلكتروني أو كلمة المرور غير صحيحة، أو الحساب مغلق مؤقتاً.");
                return View(model);
            }

            // Email confirmation check
            if (!user.EmailConfirmed && !user.Username.Equals("admin", StringComparison.OrdinalIgnoreCase))
            {
                ModelState.AddModelError(string.Empty, "الرجاء تأكيد البريد الإلكتروني الخاص بك أولاً قبل تسجيل الدخول.");
                return View(model);
            }

            // MFA Redirect Flow
            if (user.RequiresMfa)
            {
                TempData["PreMfaUserId"] = user.Id.ToString();
                TempData["RememberMeMfa"] = model.RememberMe;
                return RedirectToAction("VerifyMFA", new { returnUrl });
            }

            // Session Fixation Protection: Sign out first (Point 3/v3 & Point 3/v4)
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            // Device Identifier binding (Point 4/v3 & Point 3/v4)
            string? deviceId = Request.Cookies["__Host-Device-Id"];
            if (string.IsNullOrEmpty(deviceId))
            {
                deviceId = Guid.NewGuid().ToString("N");
                var cookieOptions = new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Strict,
                    Expires = DateTimeOffset.UtcNow.AddYears(1)
                };
                Response.Cookies.Append("__Host-Device-Id", deviceId, cookieOptions);
            }

            // Establish Identity claims
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim("TenantId", user.TenantId.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.GivenName, $"{user.FirstName} {user.LastName}"),
                new Claim(ClaimTypes.Role, user.Role.ToString()),
                new Claim("DeviceId", deviceId)
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var authProperties = new AuthenticationProperties
            {
                IsPersistent = model.RememberMe,
                ExpiresUtc = model.RememberMe ? DateTimeOffset.UtcNow.AddDays(14) : DateTimeOffset.UtcNow.AddHours(8)
            };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties);

            _logger.LogInformation("Authenticated user session created for '{Username}' with DeviceId {DeviceId}", user.Username, deviceId);

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return RedirectToAction("Index", "Dashboard");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred during login action for user '{User}'", model.UsernameOrEmail);
            ModelState.AddModelError(string.Empty, "حدث خطأ أمني أثناء محاولة تسجيل الدخول. يرجى مراجعة المسؤول.");
            return View(model);
        }
    }

    [HttpGet]
    public IActionResult VerifyMFA(string? returnUrl = null)
    {
        if (TempData["PreMfaUserId"] == null)
        {
            return RedirectToAction("Login");
        }

        TempData.Keep("PreMfaUserId");
        TempData.Keep("RememberMeMfa");

        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> VerifyMFA(string code, string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;

        var preMfaUserIdStr = TempData["PreMfaUserId"] as string;
        if (string.IsNullOrEmpty(preMfaUserIdStr) || !Guid.TryParse(preMfaUserIdStr, out var userId))
        {
            return RedirectToAction("Login");
        }

        bool rememberMe = TempData["RememberMeMfa"] as bool? ?? false;

        TempData.Keep("PreMfaUserId");
        TempData.Keep("RememberMeMfa");

        if (string.IsNullOrWhiteSpace(code))
        {
            ModelState.AddModelError(string.Empty, "يرجى إدخال رمز التحقق.");
            return View();
        }

        bool isValid = await _identityService.VerifyMfaCodeAsync(userId, code);
        if (!isValid)
        {
            isValid = await _identityService.VerifyMfaRecoveryCodeAsync(userId, code);
        }

        if (!isValid)
        {
            ModelState.AddModelError(string.Empty, "رمز التحقق أو كود الاسترداد غير صحيح.");
            return View();
        }

        var user = await _userService.GetUserByIdAsync(userId);
        if (user == null || !user.IsActive)
        {
            return RedirectToAction("Login");
        }

        // Session Fixation Protection: Sign out first (Point 3/v3 & Point 3/v4)
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

        // Device Identifier binding (Point 4/v3 & Point 3/v4)
        string? deviceId = Request.Cookies["__Host-Device-Id"];
        if (string.IsNullOrEmpty(deviceId))
        {
            deviceId = Guid.NewGuid().ToString("N");
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTimeOffset.UtcNow.AddYears(1)
            };
            Response.Cookies.Append("__Host-Device-Id", deviceId, cookieOptions);
        }

        var claims = new List<System.Security.Claims.Claim>
        {
            new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.NameIdentifier, user.Id.ToString()),
            new System.Security.Claims.Claim("TenantId", user.TenantId.ToString()),
            new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Name, user.Username),
            new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Email, user.Email),
            new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.GivenName, user.FullName),
            new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Role, user.Role.ToString()),
            new System.Security.Claims.Claim("DeviceId", deviceId)
        };

        var claimsIdentity = new System.Security.Claims.ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var authProperties = new AuthenticationProperties
        {
            IsPersistent = rememberMe,
            ExpiresUtc = rememberMe ? DateTimeOffset.UtcNow.AddDays(14) : DateTimeOffset.UtcNow.AddHours(8)
        };

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(claimsIdentity),
            authProperties);

        _logger.LogInformation("MFA Login Success for user '{Username}' with DeviceId {DeviceId}", user.Username, deviceId);

        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
        {
            return Redirect(returnUrl);
        }

        return RedirectToAction("Index", "Dashboard");
    }

    [HttpGet]
    public async Task<IActionResult> Logout()
    {
        string username = User.Identity?.Name ?? "Unknown";
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        
        string ipAddress = HttpContext.Connection?.RemoteIpAddress?.ToString() ?? "unknown";
        string userAgent = Request.Headers != null && Request.Headers.ContainsKey("User-Agent") ? Request.Headers["User-Agent"].ToString() : "unknown";

        if (userIdClaim != null && Guid.TryParse(userIdClaim.Value, out var userId))
        {
            // Session Revocation: Find session and revoke it
            var sessions = await _sessionService.GetActiveSessionsForUserAsync(userId);
            var currentSession = sessions.FirstOrDefault(s => s.IpAddress == ipAddress && s.UserAgent == userAgent);
            if (currentSession != null)
            {
                await _sessionService.RevokeSessionAsync(currentSession.Token, userId);
            }
            await _auditService.LogActivityAsync(userId, "Logout Success", ipAddress, userAgent);
        }

        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        _logger.LogInformation("User '{Username}' signed out successfully.", username);
        return RedirectToAction("Login");
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult ForgotPassword()
    {
        return View(new ForgotPasswordDto());
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordDto model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            var token = await _identityService.GeneratePasswordResetTokenAsync(model.Email);
            if (token != null)
            {
                // In production, send email. For travel system demonstration, we output verification details to view bag.
                var callbackUrl = Url.Action("ResetPassword", "Account", new { token, email = model.Email }, Request.Scheme);
                ViewBag.ResetLink = callbackUrl;
                ViewBag.Message = "تم إنشاء رابط استعادة كلمة المرور بنجاح (معروض بالأسفل للتجربة والمحاكاة).";
            }
            else
            {
                // To prevent user enumeration, we display a generic success message
                ViewBag.Message = "إذا كان هذا البريد مسجلاً لدينا، فقد تم إرسال كود استعادة كلمة المرور.";
            }

            return View(model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during forgot password for {Email}", model.Email);
            ModelState.AddModelError(string.Empty, "حدث خطأ أثناء معالجة الطلب.");
            return View(model);
        }
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult ResetPassword(string? token = null, string? email = null)
    {
        if (token == null || email == null)
        {
            return RedirectToAction("Login");
        }

        return View(new ResetPasswordDto { Token = token, Email = email });
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResetPassword(ResetPasswordDto model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            bool success = await _identityService.ResetPasswordWithTokenAsync(model.Email, model.Token, model.NewPassword);
            if (success)
            {
                TempData["SuccessMessage"] = "تم إعادة تعيين كلمة المرور بنجاح! يمكنك الآن تسجيل الدخول باستخدام كلمة المرور الجديدة.";
                return RedirectToAction("Login");
            }

            ModelState.AddModelError(string.Empty, "كود استعادة كلمة المرور غير صالح أو انتهت صلاحيته.");
            return View(model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resetting password for {Email}", model.Email);
            ModelState.AddModelError(string.Empty, "حدث خطأ أثناء إعادة تعيين كلمة المرور.");
            return View(model);
        }
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> VerifyEmail(Guid userId, string token)
    {
        try
        {
            bool verified = await _identityService.VerifyEmailWithTokenAsync(userId, token);
            if (verified)
            {
                TempData["SuccessMessage"] = "تم تأكيد البريد الإلكتروني وتفعيل الحساب بنجاح!";
            }
            else
            {
                TempData["ErrorMessage"] = "كود تفعيل البريد الإلكتروني غير صحيح أو منتهي الصلاحية.";
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error verifying email for user {UserId}", userId);
            TempData["ErrorMessage"] = "حدث خطأ أثناء تفعيل البريد الإلكتروني.";
        }

        return RedirectToAction("Login");
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> AccessDenied()
    {
        var userName = User.Identity?.Name ?? "Anonymous";
        var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        var agent = Request.Headers["User-Agent"].ToString();

        string userIdVal = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "system";
        Guid.TryParse(userIdVal, out var guidUserId);

        _logger.LogWarning("Access denied warning triggered for user: {User} accessing {Path}", userName, Request.Path);

        await _auditService.LogActivityAsync(
            guidUserId, 
            $"Access Denied: Attempted to access unauthorized location '{Request.Query["ReturnUrl"]}'", 
            ip, 
            agent);

        return View();
    }
}
