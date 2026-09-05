using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BusinessManagement.Application.Interfaces;
using BusinessManagement.Domain.Entities;
using BusinessManagement.Domain.Repositories;
using Microsoft.Extensions.Logging;

namespace BusinessManagement.Application.Services;

public class PermissionService : IPermissionService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<PermissionService> _logger;

    public PermissionService(IUnitOfWork unitOfWork, ILogger<PermissionService> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<bool> UserHasPermissionAsync(Guid userId, string permissionCode)
    {
        var permissions = await GetUserPermissionsAsync(userId);
        return permissions.Contains(permissionCode, StringComparer.OrdinalIgnoreCase);
    }

    public async Task<IEnumerable<string>> GetUserPermissionsAsync(Guid userId)
    {
        _logger.LogInformation("Resolving permissions for user: {UserId}", userId);
        
        var userRepo = _unitOfWork.GetRepository<User>();
        var user = await userRepo.GetByIdAsync(userId);
        if (user == null) return Enumerable.Empty<string>();

        // Admin has ALL permissions by default
        if (user.Role == Domain.Enums.UserRole.Admin)
        {
            var allPermissions = await _unitOfWork.GetRepository<Permission>().GetAllAsync();
            return allPermissions.Select(p => p.Code).ToList();
        }

        // Get permissions mapped through UserRoles -> RolePermissions -> Permissions
        var userRoles = await _unitOfWork.GetRepository<UserRole>().FindAsync(ur => ur.UserId == userId);
        var roleIds = userRoles.Select(ur => ur.RoleId).ToList();

        if (!roleIds.Any())
        {
            // Fallback: If no custom roles assigned, check user.Role enum
            string roleEnumName = user.Role.ToString();
            var standardRoles = await _unitOfWork.GetRepository<Role>().FindAsync(r => r.Name.ToLower() == roleEnumName.ToLower());
            var stdRole = standardRoles.FirstOrDefault();
            if (stdRole != null)
            {
                roleIds.Add(stdRole.Id);
            }
        }

        var rolePermissions = await _unitOfWork.GetRepository<RolePermission>().FindAsync(rp => roleIds.Contains(rp.RoleId));
        var permissionIds = rolePermissions.Select(rp => rp.PermissionId).Distinct().ToList();

        var permissions = await _unitOfWork.GetRepository<Permission>().FindAsync(p => permissionIds.Contains(p.Id));
        return permissions.Select(p => p.Code).ToList();
    }

    public async Task<bool> AddPermissionToRoleAsync(string roleName, string permissionCode)
    {
        _logger.LogInformation("Mapping permission {Permission} to role {Role}", permissionCode, roleName);
        
        var roleRepo = _unitOfWork.GetRepository<Role>();
        var permRepo = _unitOfWork.GetRepository<Permission>();
        var rpRepo = _unitOfWork.GetRepository<RolePermission>();

        var roles = await roleRepo.FindAsync(r => r.Name.ToLower() == roleName.ToLower());
        var role = roles.FirstOrDefault();
        if (role == null) return false;

        var perms = await permRepo.FindAsync(p => p.Code.ToLower() == permissionCode.ToLower());
        var permission = perms.FirstOrDefault();
        if (permission == null) return false;

        var existing = await rpRepo.FindAsync(rp => rp.RoleId == role.Id && rp.PermissionId == permission.Id);
        if (existing.Any()) return true; // Already exists

        var rolePermission = new RolePermission
        {
            RoleId = role.Id,
            PermissionId = permission.Id
        };
        await rpRepo.AddAsync(rolePermission);
        await _unitOfWork.SaveChangesAsync();
        return true;
    }

    public async Task<bool> RemovePermissionFromRoleAsync(string roleName, string permissionCode)
    {
        _logger.LogInformation("Removing permission {Permission} from role {Role}", permissionCode, roleName);

        var roleRepo = _unitOfWork.GetRepository<Role>();
        var permRepo = _unitOfWork.GetRepository<Permission>();
        var rpRepo = _unitOfWork.GetRepository<RolePermission>();

        var roles = await roleRepo.FindAsync(r => r.Name.ToLower() == roleName.ToLower());
        var role = roles.FirstOrDefault();
        if (role == null) return false;

        var perms = await permRepo.FindAsync(p => p.Code.ToLower() == permissionCode.ToLower());
        var permission = perms.FirstOrDefault();
        if (permission == null) return false;

        var existing = await rpRepo.FindAsync(rp => rp.RoleId == role.Id && rp.PermissionId == permission.Id);
        var item = existing.FirstOrDefault();
        if (item == null) return true; // Already removed

        rpRepo.Delete(item);
        await _unitOfWork.SaveChangesAsync();
        return true;
    }

    public async Task<IEnumerable<string>> GetRolePermissionsAsync(string roleName)
    {
        var roleRepo = _unitOfWork.GetRepository<Role>();
        var roles = await roleRepo.FindAsync(r => r.Name.ToLower() == roleName.ToLower());
        var role = roles.FirstOrDefault();
        if (role == null) return Enumerable.Empty<string>();

        var rolePermissions = await _unitOfWork.GetRepository<RolePermission>().FindAsync(rp => rp.RoleId == role.Id);
        var permissionIds = rolePermissions.Select(rp => rp.PermissionId).ToList();

        var permissions = await _unitOfWork.GetRepository<Permission>().FindAsync(p => permissionIds.Contains(p.Id));
        return permissions.Select(p => p.Code).ToList();
    }
}
