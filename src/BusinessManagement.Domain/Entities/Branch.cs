using System;
using System.Collections.Generic;

namespace BusinessManagement.Domain.Entities;

public class Branch
{
    public Guid Id { get; set; } = Guid.NewGuid();
    
    public Guid CompanyId { get; set; }
    public virtual Company Company { get; set; } = null!;

    public string Name { get; set; } = string.Empty;
}





