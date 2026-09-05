using System;

namespace BusinessManagement.Domain.Entities;

public class MonthlyClosing : BaseEntity
{
    public Guid TenantId { get; set; }
    public int Year { get; set; }
    public int Month { get; set; }
    public bool IsClosed { get; set; }
    public DateTime ClosedAt { get; set; }
    public string ClosedBy { get; set; } = string.Empty;
}
