using System;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using BusinessManagement.Application.DTOs;
using BusinessManagement.Application.Interfaces;
using BusinessManagement.Domain.Entities;
using BusinessManagement.Domain.Repositories;
using BusinessManagement.Shared.Security;
using Microsoft.Extensions.Logging;

namespace BusinessManagement.Application.Services;

public class IdentityService : IIdentityService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ISecurityAuditService _auditService;
    private readonly ILogger<IdentityService> _logger;

    public IdentityService(IUnitOfWork unitOfWork, IPasswordHasher passwordHasher, ISecurityAuditService auditService, ILogger<IdentityService> logger)
    {
        _unitOfWork = unitOfWork;
        _passwordHasher = passwordHasher;
        _auditService = auditService;
        _logger = logger;
    }

    public async Task<UserDto?> AuthenticateAsync(string usernameOrEmail, string password, string ipAddress, string userAgent)
    {
        _logger.LogInformation("Authenticating login attempt for user: {User}", usernameOrEmail);
        
        var repo = _unitOfWork.GetRepository<User>();
        var users = await repo.FindAsync(u => 
            (u.Username.ToLower() == usernameOrEmail.ToLower() || u.Email.ToLower() == usernameOrEmail.ToLower()) 
            && !u.IsDeleted,
            ignoreQueryFilters: true);

        var user = users.FirstOrDefault();
        if (user == null)
        {
            _logger.LogWarning("Authentication failed: User not found for {User}", usernameOrEmail);
            await _auditService.LogActivityAsync(null, $"Login Failed: User not found ({usernameOrEmail})", ipAddress, userAgent);
            return null;
        }

        if (!user.IsActive)
        {
            _logger.LogWarning("Authentication failed: Account disabled for {User}", user.Username);
            await _auditService.LogActivityAsync(user.Id, "Login Failed: Account disabled", ipAddress, userAgent);
            return null;
        }

        // Lockout Check
        if (user.LockoutEnd.HasValue && user.LockoutEnd.Value > DateTime.UtcNow)
        {
            var remainingTime = user.LockoutEnd.Value - DateTime.UtcNow;
            _logger.LogWarning("Authentication failed: Account locked out for {User}. Lockout ends at {End}", user.Username, user.LockoutEnd);
            await _auditService.LogActivityAsync(user.Id, $"Login Blocked: Locked out (Remaining: {remainingTime.TotalMinutes:F1} min)", ipAddress, userAgent);
            return null;
        }

        // Progressive Lockout Delay with Exponential Backoff (1s, 2s, 4s, 8s, 16s...)
        if (user.AccessFailedCount > 0)
        {
            int delaySeconds = (int)Math.Min(Math.Pow(2, user.AccessFailedCount - 1), 16);
            _logger.LogInformation("Applying progressive delay of {Delay}s on login attempt for '{User}'", delaySeconds, user.Username);
            await Task.Delay(TimeSpan.FromSeconds(delaySeconds));
        }

        // Verify password
        bool isPasswordValid = _passwordHasher.VerifyPassword(password, user.PasswordHash);
        if (!isPasswordValid)
        {
            _logger.LogWarning("Authentication failed: Invalid credentials for {User}", user.Username);
            await _auditService.LogActivityAsync(user.Id, "Login Failed: Invalid credentials", ipAddress, userAgent);
            await IncrementAccessFailedCountAsync(user.Id);
            return null;
        }

        // Email Verification Check
        if (!user.EmailConfirmed && !user.Username.Equals("admin", StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogWarning("Authentication failed: Email not verified for {User}", user.Username);
            await _auditService.LogActivityAsync(user.Id, "Login Failed: Email not verified", ipAddress, userAgent);
            return new UserDto { Id = user.Id, TenantId = user.TenantId, Username = user.Username, Email = user.Email, EmailConfirmed = false };
        }

        // Check if MFA is Enabled on Correct Credentials
        if (user.TwoFactorEnabled)
        {
            _logger.LogInformation("MFA verification required for user '{User}'", user.Username);
            return new UserDto
            {
                Id = user.Id,
                TenantId = user.TenantId,
                Username = user.Username,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Role = user.Role,
                DepartmentId = user.DepartmentId,
                IsActive = user.IsActive,
                RequiresMfa = true,
                TwoFactorEnabled = true
            };
        }

        // Authentication Success: Reset Failed Attempts and Lockout
        await ResetAccessFailedCountAsync(user.Id);
        
        // Track session details
        var session = new Session
        {
            UserId = user.Id,
            Token = Guid.NewGuid().ToString("N"),
            ExpiresAt = DateTime.UtcNow.AddHours(8),
            IpAddress = ipAddress,
            UserAgent = userAgent,
            CreatedBy = user.Username,
            UpdatedBy = user.Username
        };
        await _unitOfWork.GetRepository<Session>().AddAsync(session);

        await _auditService.LogActivityAsync(user.Id, "Login Success: Session started", ipAddress, userAgent);
        await _unitOfWork.SaveChangesAsync();

        var departments = await _unitOfWork.GetRepository<Department>().GetAllAsync();
        var deptName = departments.FirstOrDefault(d => d.Id == user.DepartmentId)?.Name ?? "None";

        return new UserDto
        {
            Id = user.Id,
            TenantId = user.TenantId,
            Username = user.Username,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Role = user.Role,
            DepartmentId = user.DepartmentId,
            DepartmentName = deptName,
            IsActive = user.IsActive,
            CreatedAt = user.CreatedAt,
            ProfilePictureUrl = user.ProfilePictureUrl,
            EmailConfirmed = user.EmailConfirmed
        };
    }

    public async Task<bool> IncrementAccessFailedCountAsync(Guid userId)
    {
        var repo = _unitOfWork.GetRepository<User>();
        var user = await repo.GetByIdAsync(userId);
        if (user == null) return false;

        user.AccessFailedCount++;
        user.UpdatedAt = DateTime.UtcNow;

        if (user.AccessFailedCount >= 5)
        {
            user.LockoutEnd = DateTime.UtcNow.AddMinutes(15);
            _logger.LogWarning("User locked out: {User} for 15 minutes due to too many failed attempts.", user.Username);
            await _auditService.LogActivityAsync(user.Id, "Account Lockout: 15 minutes (5 failed attempts)", "system", "system");
        }

        await _unitOfWork.SaveChangesAsync();
        return true;
    }

    public async Task ResetAccessFailedCountAsync(Guid userId)
    {
        var repo = _unitOfWork.GetRepository<User>();
        var user = await repo.GetByIdAsync(userId);
        if (user != null)
        {
            user.AccessFailedCount = 0;
            user.LockoutEnd = null;
            user.UpdatedAt = DateTime.UtcNow;
            await _unitOfWork.SaveChangesAsync();
        }
    }

    public async Task LockUserAsync(Guid userId, int minutes)
    {
        var repo = _unitOfWork.GetRepository<User>();
        var user = await repo.GetByIdAsync(userId);
        if (user != null)
        {
            user.LockoutEnd = DateTime.UtcNow.AddMinutes(minutes);
            user.UpdatedAt = DateTime.UtcNow;
            await _auditService.LogActivityAsync(user.Id, $"Forced Lockout: {minutes} minutes by administrator", "admin", "system");
            await _unitOfWork.SaveChangesAsync();
        }
    }

    public async Task<string?> GeneratePasswordResetTokenAsync(string email)
    {
        var repo = _unitOfWork.GetRepository<User>();
        var users = await repo.FindAsync(u => u.Email.ToLower() == email.ToLower() && !u.IsDeleted);
        var user = users.FirstOrDefault();
        if (user == null) return null;

        var tokenBytes = RandomNumberGenerator.GetBytes(32);
        string token = Convert.ToBase64String(tokenBytes)
            .Replace("+", "")
            .Replace("/", "")
            .Replace("=", "");

        user.PasswordResetToken = token;
        user.PasswordResetExpiry = DateTime.UtcNow.AddHours(2);
        user.UpdatedAt = DateTime.UtcNow;

        await _auditService.LogActivityAsync(user.Id, "Password Reset Token Generated", "requested password reset", "web");
        await _unitOfWork.SaveChangesAsync();
        return token;
    }

    public async Task<bool> ResetPasswordWithTokenAsync(string email, string token, string newPassword)
    {
        var repo = _unitOfWork.GetRepository<User>();
        var users = await repo.FindAsync(u => 
            u.Email.ToLower() == email.ToLower() 
            && u.PasswordResetToken == token 
            && u.PasswordResetExpiry > DateTime.UtcNow 
            && !u.IsDeleted);

        var user = users.FirstOrDefault();
        if (user == null) return false;

        user.PasswordHash = _passwordHasher.HashPassword(newPassword);
        user.PasswordResetToken = null;
        user.PasswordResetExpiry = null;
        user.AccessFailedCount = 0;
        user.LockoutEnd = null;
        user.UpdatedAt = DateTime.UtcNow;

        await _auditService.LogActivityAsync(user.Id, "Password Reset Success", "reset password using token", "web");
        await _unitOfWork.SaveChangesAsync();
        return true;
    }

    public async Task<string?> GenerateEmailVerificationTokenAsync(Guid userId)
    {
        var repo = _unitOfWork.GetRepository<User>();
        var user = await repo.GetByIdAsync(userId);
        if (user == null) return null;

        var tokenBytes = RandomNumberGenerator.GetBytes(32);
        string token = Convert.ToBase64String(tokenBytes)
            .Replace("+", "")
            .Replace("/", "")
            .Replace("=", "");

        user.EmailVerificationToken = token;
        user.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.SaveChangesAsync();
        return token;
    }

    public async Task<bool> VerifyEmailWithTokenAsync(Guid userId, string token)
    {
        var repo = _unitOfWork.GetRepository<User>();
        var user = await repo.GetByIdAsync(userId);
        if (user == null || user.EmailVerificationToken != token) return false;

        user.EmailConfirmed = true;
        user.EmailVerificationToken = null;
        user.UpdatedAt = DateTime.UtcNow;

        await _auditService.LogActivityAsync(user.Id, "Email Verified: verified successfully", "web", "system");
        await _unitOfWork.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ChangePasswordAsync(Guid userId, string currentPassword, string newPassword)
    {
        var repo = _unitOfWork.GetRepository<User>();
        var user = await repo.GetByIdAsync(userId);
        if (user == null) return false;

        bool isCurrentValid = _passwordHasher.VerifyPassword(currentPassword, user.PasswordHash);
        if (!isCurrentValid) return false;

        // Password Complexity Check (Point 1/v2)
        var passwordRegex = new System.Text.RegularExpressions.Regex(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$");
        if (!passwordRegex.IsMatch(newPassword))
        {
            throw new InvalidOperationException("Password does not meet complexity requirements.");
        }

        // Password History Check (Point 6/v2 & Point 2/v3)
        var historyRepo = _unitOfWork.GetRepository<UserPasswordHistory>();
        var histories = await historyRepo.FindAsync(h => h.UserId == userId);
        var recentPasswords = histories.OrderByDescending(h => h.CreatedAt).Take(5).ToList();
        foreach (var oldPass in recentPasswords)
        {
            if (_passwordHasher.VerifyPassword(newPassword, oldPass.PasswordHash))
            {
                throw new InvalidOperationException("You cannot reuse any of your last 5 passwords.");
            }
        }

        // Save current password hash to history
        var historyEntry = new UserPasswordHistory
        {
            UserId = userId,
            PasswordHash = user.PasswordHash,
            CreatedAt = DateTime.UtcNow
        };
        await historyRepo.AddAsync(historyEntry);

        user.PasswordHash = _passwordHasher.HashPassword(newPassword);
        user.UpdatedAt = DateTime.UtcNow;

        await _auditService.LogActivityAsync(user.Id, "Password Changed: manually updated", "profile", "system");
        await _unitOfWork.SaveChangesAsync();
        return true;
    }

    public async Task<bool> UpdateProfileAsync(Guid userId, string firstName, string lastName, string? profilePictureUrl)
    {
        var repo = _unitOfWork.GetRepository<User>();
        var user = await repo.GetByIdAsync(userId);
        if (user == null) return false;

        user.FirstName = firstName;
        user.LastName = lastName;
        if (profilePictureUrl != null)
        {
            user.ProfilePictureUrl = profilePictureUrl;
        }
        user.UpdatedAt = DateTime.UtcNow;

        await _auditService.LogActivityAsync(user.Id, "Profile Updated: details updated", "profile", "system");
        await _unitOfWork.SaveChangesAsync();
        return true;
    }

    public async Task<string> GenerateMfaSecretAsync(Guid userId)
    {
        var repo = _unitOfWork.GetRepository<User>();
        var user = await repo.GetByIdAsync(userId);
        if (user == null) throw new InvalidOperationException("User not found.");

        string secret = BusinessManagement.Shared.Security.TotpHelper.GenerateSecretKey();
        user.TwoFactorSecret = secret;
        user.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync();

        return secret;
    }

    public async Task<bool> EnableMfaAsync(Guid userId, string code)
    {
        var repo = _unitOfWork.GetRepository<User>();
        var user = await repo.GetByIdAsync(userId);
        if (user == null || string.IsNullOrEmpty(user.TwoFactorSecret)) return false;

        bool isValid = BusinessManagement.Shared.Security.TotpHelper.VerifyCode(user.TwoFactorSecret, code);
        if (!isValid) return false;

        user.TwoFactorEnabled = true;
        user.UpdatedAt = DateTime.UtcNow;
        await _auditService.LogActivityAsync(user.Id, "MFA Enabled: TOTP configured", "profile", "web");
        await _unitOfWork.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DisableMfaAsync(Guid userId)
    {
        var repo = _unitOfWork.GetRepository<User>();
        var user = await repo.GetByIdAsync(userId);
        if (user == null) return false;

        user.TwoFactorEnabled = false;
        user.TwoFactorSecret = null;
        user.UpdatedAt = DateTime.UtcNow;
        await _auditService.LogActivityAsync(user.Id, "MFA Disabled: TOTP removed", "profile", "web");
        await _unitOfWork.SaveChangesAsync();
        return true;
    }

    public async Task<bool> VerifyMfaCodeAsync(Guid userId, string code)
    {
        var repo = _unitOfWork.GetRepository<User>();
        var user = await repo.GetByIdAsync(userId);
        if (user == null || !user.TwoFactorEnabled || string.IsNullOrEmpty(user.TwoFactorSecret)) return false;

        return BusinessManagement.Shared.Security.TotpHelper.VerifyCode(user.TwoFactorSecret, code);
    }

    public async Task<System.Collections.Generic.IEnumerable<string>> GenerateMfaRecoveryCodesAsync(Guid userId)
    {
        var userRepo = _unitOfWork.GetRepository<User>();
        var user = await userRepo.GetByIdAsync(userId);
        if (user == null) return Array.Empty<string>();

        var recoveryRepo = _unitOfWork.GetRepository<UserMfaRecoveryCode>();
        
        // Remove existing codes
        var existing = await recoveryRepo.FindAsync(r => r.UserId == userId);
        foreach (var codeObj in existing)
        {
            recoveryRepo.Delete(codeObj);
        }

        var plainCodes = new System.Collections.Generic.List<string>();
        for (int i = 0; i < 5; i++)
        {
            // Generate a secure random 8-character code
            byte[] bytes = RandomNumberGenerator.GetBytes(4);
            string code = Convert.ToHexString(bytes).ToUpperInvariant();
            plainCodes.Add(code);

            var recoveryCode = new UserMfaRecoveryCode
            {
                UserId = userId,
                CodeHash = HashRecoveryCode(code),
                CreatedAt = DateTime.UtcNow
            };
            await recoveryRepo.AddAsync(recoveryCode);
        }

        await _auditService.LogActivityAsync(userId, "MFA Recovery Codes Generated", "profile", "web");
        await _unitOfWork.SaveChangesAsync();

        return plainCodes;
    }

    public async Task<bool> VerifyMfaRecoveryCodeAsync(Guid userId, string recoveryCode)
    {
        if (string.IsNullOrWhiteSpace(recoveryCode)) return false;

        var recoveryRepo = _unitOfWork.GetRepository<UserMfaRecoveryCode>();
        string codeHash = HashRecoveryCode(recoveryCode.Trim().ToUpperInvariant());

        var matches = await recoveryRepo.FindAsync(r => r.UserId == userId && r.CodeHash == codeHash && r.ConsumedAt == null);
        var match = matches.FirstOrDefault();

        if (match == null) return false;

        match.ConsumedAt = DateTime.UtcNow;
        recoveryRepo.Update(match);

        await _auditService.LogActivityAsync(userId, "MFA Recovery Code Consumed", "mfa login", "web");
        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    private string HashRecoveryCode(string code)
    {
        string mfaKey = Environment.GetEnvironmentVariable("APP_MFA_RECOVERY_KEY") ?? "SecureDefaultMfaRecoveryKey123!@#";
        byte[] keyBytes = System.Text.Encoding.UTF8.GetBytes(mfaKey);
        using var hmac = new HMACSHA256(keyBytes);
        byte[] hashBytes = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(code));
        return Convert.ToHexString(hashBytes).ToLowerInvariant();
    }
}
