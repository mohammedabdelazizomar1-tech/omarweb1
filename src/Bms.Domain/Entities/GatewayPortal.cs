using System;
using Bms.Domain.Enums;

namespace Bms.Domain.Entities;

public class GatewayPortal : BaseEntity
{
    public Guid TenantId { get; set; }
    public DateOnly TransactionDate { get; set; }
    public PortalServiceType ServiceType { get; set; }
    public int Count { get; set; }
    public decimal Amount { get; set; }
    
    public Guid? PartnerId { get; set; }
    public virtual Partner? Partner { get; set; }

    public string Notes { get; set; } = string.Empty;
}
