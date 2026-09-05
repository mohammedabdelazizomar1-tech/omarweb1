using System;
using System.Threading.Tasks;
using BusinessManagement.Application.Interfaces;
using BusinessManagement.Domain.Entities;
using BusinessManagement.Domain.Repositories;
using Microsoft.Extensions.Logging;

namespace BusinessManagement.Application.Services;

public class SecurityAuditService : ISecurityAuditService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<SecurityAuditService> _logger;

    public SecurityAuditService(IUnitOfWork unitOfWork, ILogger<SecurityAuditService> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task LogActivityAsync(Guid? userId, string action, string ipAddress, string userAgent)
    {
        _logger.LogInformation("Security Activity: User '{User}', Action '{Action}', IP '{IP}'", userId, action, ipAddress);
        try
        {
            var log = new ActivityLog
            {
                UserId = userId?.ToString() ?? "system",
                Action = action,
                IpAddress = ipAddress,
                UserAgent = userAgent,
                MachineName = Environment.MachineName,
                CreatedAt = DateTime.UtcNow
            };
            await _unitOfWork.GetRepository<ActivityLog>().AddAsync(log);
            await _unitOfWork.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to write activity log for action '{Action}'", action);
        }
    }

    public async Task LogDataChangeAsync(Guid userId, string entityName, Guid entityId, string changedColumns, string oldValues, string newValues)
    {
        _logger.LogInformation("Data Change: User '{User}', Entity '{Entity}', ID '{ID}'", userId, entityName, entityId);
        try
        {
            var log = new AuditLog
            {
                EntityName = entityName,
                EntityId = entityId,
                ChangedColumns = changedColumns,
                OldValues = oldValues,
                NewValues = newValues,
                UserId = userId.ToString(),
                CreatedAt = DateTime.UtcNow
            };
            await _unitOfWork.GetRepository<AuditLog>().AddAsync(log);
            await _unitOfWork.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to write audit data log for entity '{Entity}'", entityName);
        }
    }
}
