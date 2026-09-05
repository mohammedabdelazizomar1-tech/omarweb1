using System;
using System.Collections.Generic;

using BusinessManagement.Domain.Enums;

namespace BusinessManagement.Domain.Entities;

public class User : BaseEntity
{
    public BusinessManagement.Domain.Enums.UserRole Role { get; set; } = BusinessManagement.Domain.Enums.UserRole.Employee;
    public Guid TenantId { get; set; }
    public Guid BranchId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;

    // Extended security attributes
    public string? PasswordResetToken { get; set; }
    public DateTime? PasswordResetExpiry { get; set; }
    public string? EmailVerificationToken { get; set; }
    public bool EmailConfirmed { get; set; } = false;
    public int AccessFailedCount { get; set; } = 0;
    public DateTime? LockoutEnd { get; set; }
    public string? ProfilePictureUrl { get; set; }

    // MFA & Password history attributes
    public bool TwoFactorEnabled { get; set; } = false;
    public string? TwoFactorSecret { get; set; }
    public virtual ICollection<UserMfaRecoveryCode> MfaRecoveryCodes { get; set; } = new List<UserMfaRecoveryCode>();
    public virtual ICollection<UserPasswordHistory> PasswordHistories { get; set; } = new List<UserPasswordHistory>();

    public Guid? DepartmentId { get; set; }
    public virtual Department? Department { get; set; }

    public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
}





