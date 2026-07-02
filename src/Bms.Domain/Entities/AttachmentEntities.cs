using System;

namespace Bms.Domain.Entities;

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
    public string EventType { get; set; } = string.Empty;
    public string Payload { get; set; } = "{}"; // JSONB representation
    public bool IsProcessed { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ProcessedAt { get; set; }
}
