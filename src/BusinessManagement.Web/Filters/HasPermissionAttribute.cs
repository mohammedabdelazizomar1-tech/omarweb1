using System;
using System.Security.Claims;
using System.Threading.Tasks;
using BusinessManagement.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;

namespace BusinessManagement.Web.Filters;

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = true)]
public class HasPermissionAttribute : Attribute, IAsyncAuthorizationFilter
{
    public string Permission { get; }

    public HasPermissionAttribute(string permission)
    {
        Permission = permission;
    }

    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        var user = context.HttpContext.User;
        if (user.Identity?.IsAuthenticated != true)
        {
            context.Result = new ChallengeResult();
            return;
        }

        var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
        {
            context.Result = new ForbidResult();
            return;
        }

        var permissionService = context.HttpContext.RequestServices.GetRequiredService<IPermissionService>();
        bool hasPermission = await permissionService.UserHasPermissionAsync(userId, Permission);

        if (!hasPermission)
        {
            if (context.HttpContext.Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                context.Result = new JsonResult(new { success = false, message = "عذراً، لا تمتلك الصلاحية الكافية لإتمام هذا الإجراء." })
                {
                    StatusCode = 403
                };
            }
            else
            {
                context.Result = new RedirectToActionResult("AccessDenied", "Account", null);
            }
        }
    }
}
