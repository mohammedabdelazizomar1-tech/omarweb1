using System;
using System.Collections.Generic;

namespace BusinessManagement.Domain.Entities;

public class Supplier : BaseEntity
{
    public Guid TenantId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string SupplierType { get; set; } = string.Empty; // Hotel, Flight, Bus, Visa
    public string CommercialRegister { get; set; } = string.Empty;
    public string TaxNumber { get; set; } = string.Empty;

    public virtual ICollection<SupplierContact> Contacts { get; set; } = new List<SupplierContact>();
    public virtual ICollection<SupplierContract> Contracts { get; set; } = new List<SupplierContract>();
}

public class SupplierContact : BaseEntity
{
    public Guid SupplierId { get; set; }
    public virtual Supplier Supplier { get; set; } = null!;
    
    public string ContactName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
}

public class SupplierContract : BaseEntity
{
    public Guid SupplierId { get; set; }
    public virtual Supplier Supplier { get; set; } = null!;

    public string ContractNumber { get; set; } = string.Empty;
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public string Terms { get; set; } = string.Empty;
}

public class Package : BaseEntity
{
    public Guid TenantId { get; set; }
    public string Name { get; set; } = string.Empty;
    public int NightsCount { get; set; }
    public decimal TotalBasePrice { get; set; }
}

public class Hotel : BaseEntity
{
    public Guid SupplierId { get; set; }
    public virtual Supplier Supplier { get; set; } = null!;

    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public int StarsRating { get; set; }
}

public class Flight : BaseEntity
{
    public Guid SupplierId { get; set; }
    public virtual Supplier Supplier { get; set; } = null!;

    public string AirlineName { get; set; } = string.Empty;
    public string FlightNumber { get; set; } = string.Empty;
    public string DepartureAirport { get; set; } = string.Empty;
    public string ArrivalAirport { get; set; } = string.Empty;
}

public class Bus : BaseEntity
{
    public Guid SupplierId { get; set; }
    public virtual Supplier Supplier { get; set; } = null!;

    public string TransportName { get; set; } = string.Empty;
    public string PlateNumber { get; set; } = string.Empty;
    public int Capacity { get; set; }
}

public class Visa : BaseEntity
{
    public Guid SupplierId { get; set; }
    public virtual Supplier Supplier { get; set; } = null!;

    public string VisaType { get; set; } = string.Empty; // e.g. Umrah, Tourist, Hajj
    public int TotalSlots { get; set; }
    public decimal PricePerSlot { get; set; }
}





