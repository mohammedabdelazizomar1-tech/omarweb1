using System;
using System.Collections.Generic;

using Bms.Domain.Enums;

namespace Bms.Domain.Entities;

public class User : BaseEntity
{
    public Bms.Domain.Enums.UserRole Role { get; set; } = Bms.Domain.Enums.UserRole.Employee;
    public Guid TenantId { get; set; }
    public Guid BranchId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;

    public Guid? DepartmentId { get; set; }
    public virtual Department? Department { get; set; }

    public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
}
