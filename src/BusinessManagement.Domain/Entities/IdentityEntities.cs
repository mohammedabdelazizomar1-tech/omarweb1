using System;
using System.Collections.Generic;

namespace BusinessManagement.Domain.Entities;

public class Role
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;

    public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    public virtual ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
}

public class Permission
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Code { get; set; } = string.Empty; // e.g. "finance.journal.post"
    public string Description { get; set; } = string.Empty;

    public virtual ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
}

public class UserRole
{
    public Guid UserId { get; set; }
    public virtual User User { get; set; } = null!;

    public Guid RoleId { get; set; }
    public virtual Role Role { get; set; } = null!;
}

public class RolePermission
{
    public Guid RoleId { get; set; }
    public virtual Role Role { get; set; } = null!;

    public Guid PermissionId { get; set; }
    public virtual Permission Permission { get; set; } = null!;
}

public class Session : BaseEntity
{
    public Guid UserId { get; set; }
    public virtual User User { get; set; } = null!;

    public string Token { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public bool IsRevoked { get; set; } = false;
    
    // Client details
    public string IpAddress { get; set; } = string.Empty;
    public string UserAgent { get; set; } = string.Empty;
}

public class RefreshToken : BaseEntity
{
    public Guid UserId { get; set; }
    public virtual User User { get; set; } = null!;

    public string Token { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public bool IsRevoked { get; set; } = false;
}





