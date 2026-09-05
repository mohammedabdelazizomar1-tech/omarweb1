using System;

namespace BusinessManagement.Domain.Entities;

public class ProcessedEvent
{
    public Guid EventId { get; set; } // Primary Key
    public DateTime ProcessedAt { get; set; } = DateTime.UtcNow;
    public string? DeviceId { get; set; }
}
