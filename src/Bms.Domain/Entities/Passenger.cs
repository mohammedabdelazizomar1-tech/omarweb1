using System;
using System.Collections.Generic;

namespace Bms.Domain.Entities;

public class Passenger : BaseEntity
{
    public Guid TenantId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string PassportNumber { get; set; } = string.Empty;
    public DateOnly PassportExpiry { get; set; }
    public string NationalId { get; set; } = string.Empty;
    public DateOnly? DateOfBirth { get; set; }
    public string Nationality { get; set; } = "Egyptian";
    public string Gender { get; set; } = string.Empty; // Male, Female
    public string PhoneNumber { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string EmergencyContactName { get; set; } = string.Empty;
    public string EmergencyContactPhone { get; set; } = string.Empty;

    // Navigation properties
    public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();
}
