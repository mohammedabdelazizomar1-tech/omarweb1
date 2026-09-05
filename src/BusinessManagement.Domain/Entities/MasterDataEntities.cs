using System;

namespace BusinessManagement.Domain.Entities;

public class Country : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty; // e.g. "EG", "SA"
}

public class City : BaseEntity
{
    public Guid CountryId { get; set; }
    public virtual Country Country { get; set; } = null!;
    
    public string Name { get; set; } = string.Empty;
}

public class CurrencyMaster
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Code { get; set; } = string.Empty; // e.g. "EGP", "SAR", "USD"
    public string Name { get; set; } = string.Empty;
    public string Symbol { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
}

public class ExchangeRate
{
    public Guid Id { get; set; } = Guid.NewGuid();
    
    public Guid CurrencyFromId { get; set; }
    public virtual CurrencyMaster CurrencyFrom { get; set; } = null!;

    public Guid CurrencyToId { get; set; }
    public virtual CurrencyMaster CurrencyTo { get; set; } = null!;

    public decimal Rate { get; set; }
    public DateTime EffectiveFrom { get; set; }
    public DateTime EffectiveTo { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}





