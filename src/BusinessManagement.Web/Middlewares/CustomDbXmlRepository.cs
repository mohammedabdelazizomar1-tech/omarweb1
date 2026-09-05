using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Microsoft.AspNetCore.DataProtection.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace BusinessManagement.Web.Middlewares;

public class CustomDbXmlRepository : IXmlRepository
{
    private readonly IServiceProvider _serviceProvider;

    public CustomDbXmlRepository(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public IReadOnlyCollection<XElement> GetAllElements()
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<BusinessManagement.Persistence.BusinessManagementDbContext>();
        
        // Suppress tenant query filters to retrieve global data protection keys
        var keys = context.Settings
            .IgnoreQueryFilters()
            .Where(s => s.Key.StartsWith("DPKey:"))
            .ToList();

        var list = new List<XElement>();
        foreach (var key in keys)
        {
            try
            {
                list.Add(XElement.Parse(key.Value));
            }
            catch { }
        }
        return list;
    }

    public void StoreElement(XElement element, string friendlyName)
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<BusinessManagement.Persistence.BusinessManagementDbContext>();

        string keyName = $"DPKey:{friendlyName ?? Guid.NewGuid().ToString("N")}";
        var existing = context.Settings
            .IgnoreQueryFilters()
            .FirstOrDefault(s => s.Key == keyName);

        if (existing != null)
        {
            existing.Value = element.ToString(SaveOptions.DisableFormatting);
            existing.UpdatedAt = DateTime.UtcNow;
        }
        else
        {
            context.Settings.Add(new BusinessManagement.Domain.Entities.Setting
            {
                TenantId = Guid.Empty, // System-wide global setting
                Key = keyName,
                Value = element.ToString(SaveOptions.DisableFormatting),
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "system",
                UpdatedAt = DateTime.UtcNow,
                UpdatedBy = "system"
            });
        }
        context.SaveChanges();
    }
}
