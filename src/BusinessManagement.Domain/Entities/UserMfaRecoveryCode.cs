using System;

namespace BusinessManagement.Domain.Entities;

public class UserMfaRecoveryCode : BaseEntity
{
    public Guid UserId { get; set; }
    public virtual User? User { get; set; }
    public string CodeHash { get; set; } = string.Empty;
    public DateTime? ConsumedAt { get; set; }
}
