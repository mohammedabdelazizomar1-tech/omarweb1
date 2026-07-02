using System;

namespace Bms.Domain.Entities;

public class InventoryItem : BaseEntity
{
    public Guid TenantId { get; set; }
    public string Sku { get; set; } = string.Empty; // e.g. "SRV-QR-UMRAH"
    public string Name { get; set; } = string.Empty;
    public string ItemType { get; set; } = string.Empty; // "VirtualBarcode", "PhysicalKit"
}

public class InventoryTransaction : BaseEntity
{
    public Guid InventoryItemId { get; set; }
    public virtual InventoryItem InventoryItem { get; set; } = null!;

    public DateOnly TransactionDate { get; set; }
    public int Quantity { get; set; }
    public decimal UnitCost { get; set; }
    public string TransactionType { get; set; } = string.Empty; // "Replenishment", "Consumption"
}
