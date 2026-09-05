using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using BusinessManagement.Domain.Entities;
using BusinessManagement.Domain.Repositories;
using BusinessManagement.Application.Interfaces;
using BusinessManagement.Web.Filters;

namespace BusinessManagement.Web.Controllers;

[Authorize(Policy = "roles.manage")]
public class RoleManagementController : Controller
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPermissionService _permissionService;
    private readonly ISecurityAuditService _auditService;
    private readonly ILogger<RoleManagementController> _logger;

    public RoleManagementController(
        IUnitOfWork unitOfWork,
        IPermissionService permissionService,
        ISecurityAuditService auditService,
        ILogger<RoleManagementController> logger)
    {
        _unitOfWork = unitOfWork;
        _permissionService = permissionService;
        _auditService = auditService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var roles = await _unitOfWork.GetRepository<Role>().GetAllAsync();
        
        ViewBag.SuccessMessage = TempData["SuccessMessage"];
        ViewBag.ErrorMessage = TempData["ErrorMessage"];

        return View(roles);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateRole(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            TempData["ErrorMessage"] = "اسم الدور مطلوب ولا يمكن أن يكون فارغاً.";
            return RedirectToAction("Index");
        }

        try
        {
            var repo = _unitOfWork.GetRepository<Role>();
            var existing = await repo.FindAsync(r => r.Name.ToLower() == name.Trim().ToLower());
            if (existing.Any())
            {
                TempData["ErrorMessage"] = "اسم الدور هذا مسجل بالفعل بالخادم.";
                return RedirectToAction("Index");
            }

            var newRole = new Role { Name = name.Trim() };
            await repo.AddAsync(newRole);

            string userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "system";
            Guid.TryParse(userId, out var guidUserId);
            await _auditService.LogActivityAsync(guidUserId, $"Create Role: {name}", HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown", Request.Headers["User-Agent"].ToString());

            await _unitOfWork.SaveChangesAsync();
            TempData["SuccessMessage"] = "تم إنشاء دور الصلاحية بنجاح!";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating role: {Role}", name);
            TempData["ErrorMessage"] = "حدث خطأ أثناء حفظ دور الصلاحية.";
        }

        return RedirectToAction("Index");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteRole(Guid id)
    {
        try
        {
            var repo = _unitOfWork.GetRepository<Role>();
            var role = await repo.GetByIdAsync(id);
            if (role == null)
            {
                TempData["ErrorMessage"] = "دور الصلاحية غير موجود بالخادم.";
                return RedirectToAction("Index");
            }

            // Restrict deleting system-critical roles
            if (role.Name.Equals("Admin", StringComparison.OrdinalIgnoreCase) || role.Name.Equals("Partner", StringComparison.OrdinalIgnoreCase))
            {
                TempData["ErrorMessage"] = "عذراً، لا يمكن حذف الأدوار الأساسية الخاصة بالنظام (Admin/Partner).";
                return RedirectToAction("Index");
            }

            // Remove all role permissions mappings first
            var rpRepo = _unitOfWork.GetRepository<RolePermission>();
            var mappings = await rpRepo.FindAsync(rp => rp.RoleId == id);
            foreach (var mapping in mappings)
            {
                rpRepo.Delete(mapping);
            }

            repo.Delete(role);

            string userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "system";
            Guid.TryParse(userId, out var guidUserId);
            await _auditService.LogActivityAsync(guidUserId, $"Delete Role: {role.Name}", HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown", Request.Headers["User-Agent"].ToString());

            await _unitOfWork.SaveChangesAsync();
            TempData["SuccessMessage"] = "تم حذف وإلغاء دور الصلاحية وتصفية ملحقاته بنجاح!";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting role with ID: {RoleId}", id);
            TempData["ErrorMessage"] = "حدث خطأ داخلي بالخادم أثناء محاولة حذف دور الصلاحية.";
        }

        return RedirectToAction("Index");
    }

    [HttpGet]
    public async Task<IActionResult> Permissions(Guid roleId)
    {
        var role = await _unitOfWork.GetRepository<Role>().GetByIdAsync(roleId);
        if (role == null) return NotFound("دور الصلاحية غير موجود.");

        var allPermissions = await _unitOfWork.GetRepository<Permission>().GetAllAsync();
        var rolePermissions = await _unitOfWork.GetRepository<RolePermission>().FindAsync(rp => rp.RoleId == roleId);
        var mappedPermissionIds = rolePermissions.Select(rp => rp.PermissionId).ToList();

        ViewBag.Role = role;
        ViewBag.MappedPermissionIds = mappedPermissionIds;
        ViewBag.SuccessMessage = TempData["SuccessMessage"];
        ViewBag.ErrorMessage = TempData["ErrorMessage"];

        return View(allPermissions);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateRolePermissions(Guid roleId, List<Guid> selectedPermissions)
    {
        var role = await _unitOfWork.GetRepository<Role>().GetByIdAsync(roleId);
        if (role == null) return NotFound();

        try
        {
            var rpRepo = _unitOfWork.GetRepository<RolePermission>();
            
            // 1. Remove all existing permissions mapped to this role
            var existingMappings = await rpRepo.FindAsync(rp => rp.RoleId == roleId);
            foreach (var mapping in existingMappings)
            {
                rpRepo.Delete(mapping);
            }

            // 2. Add newly selected permissions
            if (selectedPermissions != null)
            {
                foreach (var permId in selectedPermissions)
                {
                    await rpRepo.AddAsync(new RolePermission
                    {
                        RoleId = roleId,
                        PermissionId = permId
                    });
                }
            }

            string userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "system";
            Guid.TryParse(userId, out var guidUserId);
            await _auditService.LogActivityAsync(guidUserId, $"Update Role Permissions for: {role.Name}", HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown", Request.Headers["User-Agent"].ToString());

            await _unitOfWork.SaveChangesAsync();
            TempData["SuccessMessage"] = "تم تحديث وحفظ صلاحيات الدور بنجاح!";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating permissions mapping for role: {Role}", role.Name);
            TempData["ErrorMessage"] = "حدث خطأ أثناء تعديل وحفظ مصفوفة الصلاحيات.";
        }

        return RedirectToAction("Permissions", new { roleId });
    }
}
