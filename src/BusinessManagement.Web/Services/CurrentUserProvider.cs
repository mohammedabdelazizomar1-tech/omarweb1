using System;
using System.Security.Claims;
using BusinessManagement.Domain;
using Microsoft.AspNetCore.Http;

namespace BusinessManagement.Web.Services;

public class CurrentUserProvider : ICurrentUserProvider
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserProvider(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public string GetCurrentUsername()
    {
        return _httpContextAccessor.HttpContext?.User?.Identity?.Name ?? "system";
    }

    public Guid GetCurrentTenantId()
    {
        var claim = _httpContextAccessor.HttpContext?.User?.FindFirst("TenantId");
        return claim != null && Guid.TryParse(claim.Value, out var tenantId) ? tenantId : Guid.Empty;
    }
}
