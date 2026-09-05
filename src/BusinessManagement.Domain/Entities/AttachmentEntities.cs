using System;

namespace BusinessManagement.Domain.Entities;

public class Attachment : BaseEntity
{
    public Guid TenantId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty; // S3 URI or local filesystem path
    public string ContentType { get; set; } = string.Empty;
    public long FileSizeBytes { get; set; }
}

public class AttachmentLink
{
    public Guid Id { get; set; } = Guid.NewGuid();
    
    public Guid AttachmentId { get; set; }
    public virtual Attachment Attachment { get; set; } = null!;

    public string TargetEntityName { get; set; } = string.Empty; // e.g. "Passenger", "Booking"
    public Guid TargetEntityId { get; set; }
}
public class OutboxMessage
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid EventId { get; set; } = Guid.NewGuid();
    public int EventVersion { get; set; } = 1;
    public Guid AggregateId { get; set; }
    public string AggregateType { get; set; } = string.Empty;
    public string EventType { get; set; } = string.Empty;
    public string Payload { get; set; } = "{}";
    
    // Status State Machine: "Pending", "Processing", "Completed", "Failed", "DeadLetter"
    public string Status { get; set; } = "Pending";
    public int RetryCount { get; set; } = 0;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ProcessedAt { get; set; }
    public DateTime? NextRetryAt { get; set; }
    
    // Telemetry & Tracing
    public string? DeviceId { get; set; }
    public string? CorrelationId { get; set; }
    public string? LastError { get; set; }
    
    // Auto-incremented sequence identifier managed by PostgreSQL
    public long SequenceId { get; set; }
}





