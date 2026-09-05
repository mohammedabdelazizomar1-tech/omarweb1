using System;
using System.Threading.Tasks;
using BusinessManagement.Application.DTOs;

namespace BusinessManagement.Application.Interfaces;

public interface IIdentityService
{
    Task<UserDto?> AuthenticateAsync(string usernameOrEmail, string password, string ipAddress, string userAgent);
    Task<bool> IncrementAccessFailedCountAsync(Guid userId);
    Task ResetAccessFailedCountAsync(Guid userId);
    Task LockUserAsync(Guid userId, int minutes);
    Task<string?> GeneratePasswordResetTokenAsync(string email);
    Task<bool> ResetPasswordWithTokenAsync(string email, string token, string newPassword);
    Task<string?> GenerateEmailVerificationTokenAsync(Guid userId);
    Task<bool> VerifyEmailWithTokenAsync(Guid userId, string token);
    Task<bool> ChangePasswordAsync(Guid userId, string currentPassword, string newPassword);
    Task<bool> UpdateProfileAsync(Guid userId, string firstName, string lastName, string? profilePictureUrl);
    Task<string> GenerateMfaSecretAsync(Guid userId);
    Task<bool> EnableMfaAsync(Guid userId, string code);
    Task<bool> DisableMfaAsync(Guid userId);
    Task<bool> VerifyMfaCodeAsync(Guid userId, string code);
    Task<System.Collections.Generic.IEnumerable<string>> GenerateMfaRecoveryCodesAsync(Guid userId);
    Task<bool> VerifyMfaRecoveryCodeAsync(Guid userId, string recoveryCode);
}
