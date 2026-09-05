using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BusinessManagement.Application.Interfaces;

public interface IPermissionService
{
    Task<bool> UserHasPermissionAsync(Guid userId, string permissionCode);
    Task<IEnumerable<string>> GetUserPermissionsAsync(Guid userId);
    Task<bool> AddPermissionToRoleAsync(string roleName, string permissionCode);
    Task<bool> RemovePermissionFromRoleAsync(string roleName, string permissionCode);
    Task<IEnumerable<string>> GetRolePermissionsAsync(string roleName);
}
