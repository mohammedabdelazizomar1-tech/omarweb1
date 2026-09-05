using System;
using System.Security.Claims;
using System.Threading.Tasks;
using BusinessManagement.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;

namespace BusinessManagement.Web.Authorization;

public class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
{
    private readonly IPermissionService _permissionService;

    public PermissionAuthorizationHandler(IPermissionService permissionService)
    {
        _permissionService = permissionService;
    }

    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionRequirement requirement)
    {
        var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null) return;

        if (Guid.TryParse(userIdClaim.Value, out var userId))
        {
            if (await _permissionService.UserHasPermissionAsync(userId, requirement.Permission))
            {
                context.Succeed(requirement);
            }
        }
    }
}
