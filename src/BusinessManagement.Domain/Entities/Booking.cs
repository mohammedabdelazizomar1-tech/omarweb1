using System;
using System.Collections.Generic;
using System.Linq;

namespace BusinessManagement.Domain.Entities;

public class Booking : BaseEntity
{
    public Guid TenantId { get; set; }
    public Guid BranchId { get; set; }
    public string BookingNumber { get; set; } = string.Empty; // e.g. B-001002
    
    // Passenger Relationship
    public Guid PassengerId { get; set; }
    public virtual Passenger Passenger { get; set; } = null!;

    // Partner Relationship (Attribution)
    public Guid PartnerId { get; set; }
    public virtual Partner Partner { get; set; } = null!;

    // Package catalog Relationship
    public Guid PackageId { get; set; }
    public virtual Package Package { get; set; } = null!;

    public DateOnly TravelDate { get; set; }
    public string CurrentStatus { get; set; } = "Draft"; // Draft, Confirmed, Issued, Cancelled, Completed
    public string Notes { get; set; } = string.Empty;
    public bool HasQrCode { get; set; }

    // Navigation properties for normalized tables
    public virtual BookingCost BookingCost { get; set; } = new BookingCost();
    public virtual BookingSnapshot? BookingSnapshot { get; set; }
    public virtual ICollection<BookingPayment> BookingPayments { get; set; } = new List<BookingPayment>();
    public virtual ICollection<BookingStatusHistory> StatusHistory { get; set; } = new List<BookingStatusHistory>();

    // ========================================================
    // BACKWARD COMPATIBILITY DELEGATED PROPERTIES (3NF ALIGNMENT)
    // ========================================================
    
    public decimal NetCost 
    { 
        get => BookingCost.NetCost; 
        set => BookingCost.NetCost = value; 
    }

    public decimal BarcodeCost 
    { 
        get => BookingCost.BarcodeCost; 
        set => BookingCost.BarcodeCost = value; 
    }

    public decimal CompanyCost 
    { 
        get => BookingCost.CompanyMarkup; 
        set => BookingCost.CompanyMarkup = value; 
    }

    public decimal AirportCost 
    { 
        get => BookingCost.AirportCost; 
        set => BookingCost.AirportCost = value; 
    }

    public decimal ProgramCost 
    { 
        get => BookingCost.HotelCost; 
        set => BookingCost.HotelCost = value; 
    }

    public decimal TicketCost 
    { 
        get => BookingCost.TicketCost; 
        set => BookingCost.TicketCost = value; 
    }

    public decimal BusCost 
    { 
        get => BookingCost.BusCost; 
        set => BookingCost.BusCost = value; 
    }

    public decimal TotalCost 
    { 
        get => NetCost + BarcodeCost + CompanyCost + AirportCost + ProgramCost + TicketCost + BusCost;
        set { }
    }

    public decimal SellingPrice { get; set; }

    public decimal NetProfit 
    { 
        get => SellingPrice - TotalCost;
        set { }
    }

    public decimal PaymentsCollected { get; set; }

    public decimal RemainingBalance { get; set; }

    public string? GroupNumber { get; set; }
}

public class BookingCost
{
    public Guid Id { get; set; } = Guid.NewGuid();
    
    public Guid BookingId { get; set; }
    public virtual Booking Booking { get; set; } = null!;

    // Cost Breakdowns
    public decimal NetCost { get; set; }
    public decimal BarcodeCost { get; set; }
    public decimal CompanyMarkup { get; set; }
    public decimal AirportCost { get; set; }
    public decimal HotelCost { get; set; }
    public decimal TicketCost { get; set; }
    public decimal BusCost { get; set; }
}

public class BookingSnapshot
{
    public Guid Id { get; set; } = Guid.NewGuid();
    
    public Guid BookingId { get; set; }
    public virtual Booking Booking { get; set; } = null!;

    public string FrozenHotelName { get; set; } = string.Empty;
    public string FrozenAirline { get; set; } = string.Empty;
    public decimal FrozenTicketPrice { get; set; }
    public string FrozenSupplierDetails { get; set; } = "{}"; // JSONB representation of supplier contracts
}

public class BookingPayment : BaseEntity
{
    public Guid BookingId { get; set; }
    public virtual Booking Booking { get; set; } = null!;

    public decimal Amount { get; set; }
    public string Currency { get; set; } = "USD";
    public string PaymentMethod { get; set; } = "Cash"; // Cash, BankTransfer, Visa
    public string ReceiptNumber { get; set; } = string.Empty;
    public DateOnly PaymentDate { get; set; }
}

public class BookingStatusHistory
{
    public Guid Id { get; set; } = Guid.NewGuid();
    
    public Guid BookingId { get; set; }
    public virtual Booking Booking { get; set; } = null!;

    public string FromStatus { get; set; } = string.Empty;
    public string ToStatus { get; set; } = string.Empty;
    public DateTime ChangedAt { get; set; } = DateTime.UtcNow;
    public string ChangedBy { get; set; } = "system";
    public string Notes { get; set; } = string.Empty;
}





