using System;

namespace BusinessManagement.Domain.Entities;

public class AuditLog
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TenantId { get; set; }
    
    public string EntityName { get; set; } = string.Empty;
    public Guid EntityId { get; set; }
    
    public string ChangedColumns { get; set; } = "{}"; // JSONB formatted string
    public string OldValues { get; set; } = "{}";     // JSONB formatted string
    public string NewValues { get; set; } = "{}";     // JSONB formatted string
    
    public string UserId { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? LogHash { get; set; }
    public string? PreviousLogHash { get; set; }
}

public class ActivityLog
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TenantId { get; set; }
    public string UserId { get; set; } = string.Empty;
    
    public string Action { get; set; } = string.Empty;
    public string IpAddress { get; set; } = string.Empty;
    public string UserAgent { get; set; } = string.Empty;
    public string MachineName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class Setting : BaseEntity
{
    public Guid TenantId { get; set; }
    public string Key { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string GroupName { get; set; } = string.Empty; // e.g. "Security", "SMTP"
}

public class Notification
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TenantId { get; set; }
    public Guid UserId { get; set; }
    
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string Payload { get; set; } = "{}"; // JSONB formatted payload string
    public bool IsRead { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}





