using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BusinessManagement.Domain.Entities;

namespace BusinessManagement.Application.Interfaces;

public interface ISessionService
{
    Task<Session?> GetSessionByTokenAsync(string token);
    Task<IEnumerable<Session>> GetActiveSessionsForUserAsync(Guid userId);
    Task<bool> RevokeSessionAsync(string token, Guid userId);
    Task<bool> RevokeAllSessionsForUserExceptCurrentAsync(Guid userId, string currentToken);
    Task<bool> RevokeAllSessionsForUserAsync(Guid userId);
}
