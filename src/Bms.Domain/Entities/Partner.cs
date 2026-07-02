using System;
using System.Collections.Generic;

namespace Bms.Domain.Entities;

public class Partner : BaseEntity
{
    public Guid TenantId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty; // Mapped to System Login Username
    public int QuotaLimit { get; set; } // Max allocated barcode/visa slots
    public bool IsActive { get; set; } = true;

    // Corporate metadata
    public string Phone { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string CommercialRegister { get; set; } = string.Empty;
    public string TaxNumber { get; set; } = string.Empty;

    // Navigation properties
    public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();
}
