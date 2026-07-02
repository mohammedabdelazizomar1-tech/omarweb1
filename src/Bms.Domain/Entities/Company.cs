using System;
using System.Collections.Generic;

namespace Bms.Domain.Entities;

public class Company
{
    public Guid Id { get; set; } = Guid.NewGuid();
    
    public Guid TenantId { get; set; }
    public virtual Tenant Tenant { get; set; } = null!;

    public string Name { get; set; } = string.Empty;
    public string CommercialRegister { get; set; } = string.Empty;
    public string TaxNumber { get; set; } = string.Empty;

    public virtual ICollection<Branch> Branches { get; set; } = new List<Branch>();
}
