using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using BusinessManagement.Application.DTOs;
using BusinessManagement.Application.Interfaces;

namespace BusinessManagement.Web.Controllers;

[Authorize(Roles = "Admin,Manager")]
public class UsersController : Controller
{
    private readonly IUserService _userService;
    private readonly ILogger<UsersController> _logger;

    public UsersController(IUserService _userService, ILogger<UsersController> logger)
    {
        this._userService = _userService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        ViewData["PageHeader"] = "إدارة الحسابات وبيانات تسجيل الدخول";
        _logger.LogInformation("Loading employee directory. User: {User}", User.Identity?.Name);
        try
        {
            var users = await _userService.GetAllUsersAsync();
            var departments = await _userService.GetAllDepartmentsAsync();
            
            ViewBag.Departments = departments;
            return View(users);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred loading employees dashboard.");
            ViewBag.Departments = new List<DepartmentDto>();
            return View(new List<UserDto>());
        }
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        _logger.LogInformation("Retrieving employee registration form.");
        try
        {
            var depts = await _userService.GetAllDepartmentsAsync();
            ViewBag.Departments = depts;
            return PartialView("_Create", new CreateUserDto());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error preparing create employee modal.");
            return StatusCode(500, "حدث خطأ أثناء تحميل نموذج إضافة الحساب.");
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateUserDto model)
    {
        if (!ModelState.IsValid)
        {
            return Json(new { success = false, message = "يرجى التثبت من صحة البيانات المدخلة في النموذج." });
        }

        try
        {
            await _userService.CreateUserAsync(model);
            return Json(new { success = true, message = "تم إنشاء حساب المستخدم وبيانات الدخول بنجاح!" });
        }
        catch (InvalidOperationException ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error registering employee: {Username}", model.Username);
            return Json(new { success = false, message = "حدث خطأ داخلي أثناء إنشاء حساب المستخدم." });
        }
    }

    [HttpGet]
    public async Task<IActionResult> Edit(Guid id)
    {
        _logger.LogInformation("Retrieving edit employee form for ID: {UserId}", id);
        try
        {
            var user = await _userService.GetUserByIdAsync(id);
            if (user == null)
            {
                return NotFound("المستخدم غير موجود.");
            }

            var depts = await _userService.GetAllDepartmentsAsync();
            ViewBag.Departments = depts;

            var model = new UpdateUserDto
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Role = user.Role,
                DepartmentId = user.DepartmentId,
                IsActive = user.IsActive
            };

            return PartialView("_Edit", model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading edit form details for employee ID: {UserId}", id);
            return StatusCode(500, "حدث خطأ أثناء تحميل بيانات الحساب.");
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(UpdateUserDto model)
    {
        if (!ModelState.IsValid)
        {
            return Json(new { success = false, message = "يرجى التثبت من صحة الحقول المدخلة." });
        }

        try
        {
            await _userService.UpdateUserAsync(model);
            return Json(new { success = true, message = "تم تحديث بيانات الحساب وكلمة المرور بنجاح!" });
        }
        catch (InvalidOperationException ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating employee: {UserId}", model.Id);
            return Json(new { success = false, message = "حدث خطأ داخلي أثناء حفظ بيانات الحساب." });
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleStatus(Guid id)
    {
        _logger.LogInformation("Toggling account status for ID: {UserId} by {User}", id, User.Identity?.Name);
        try
        {
            await _userService.ToggleUserStatusAsync(id);
            return Json(new { success = true, message = "تم تغيير حالة الحساب بنجاح!" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error toggling status for employee ID: {UserId}", id);
            return Json(new { success = false, message = "حدث خطأ أثناء تغيير حالة الحساب." });
        }
    }

    [HttpGet]
    public IActionResult CreateDepartment()
    {
        _logger.LogInformation("Retrieving department creation modal form.");
        return PartialView("_CreateDepartment", new CreateDepartmentDto());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateDepartment(CreateDepartmentDto model)
    {
        if (!ModelState.IsValid)
        {
            return Json(new { success = false, message = "Input validation failed. Please check form parameters." });
        }

        try
        {
            await _userService.CreateDepartmentAsync(model);
            return Json(new { success = true, message = "New organizational department created successfully!" });
        }
        catch (InvalidOperationException ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating department: {DeptName}", model.Name);
            return Json(new { success = false, message = "An internal server error occurred while creating department." });
        }
    }
}





