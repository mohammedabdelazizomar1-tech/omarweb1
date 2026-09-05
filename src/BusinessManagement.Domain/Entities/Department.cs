using System;
using System.Collections.Generic;

namespace BusinessManagement.Domain.Entities;

public class Department : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    // Navigation property
    public virtual ICollection<User> Users { get; set; } = new List<User>();
}





