using System;
using System.Threading.Tasks;

namespace BusinessManagement.Application.Interfaces;

public interface ISecurityAuditService
{
    Task LogActivityAsync(Guid? userId, string action, string ipAddress, string userAgent);
    Task LogDataChangeAsync(Guid userId, string entityName, Guid entityId, string changedColumns, string oldValues, string newValues);
}
