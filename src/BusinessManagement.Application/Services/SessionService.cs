using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BusinessManagement.Application.Interfaces;
using BusinessManagement.Domain.Entities;
using BusinessManagement.Domain.Repositories;
using Microsoft.Extensions.Logging;

namespace BusinessManagement.Application.Services;

public class SessionService : ISessionService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<SessionService> _logger;

    public SessionService(IUnitOfWork unitOfWork, ILogger<SessionService> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Session?> GetSessionByTokenAsync(string token)
    {
        var sessions = await _unitOfWork.GetRepository<Session>().FindAsync(s => s.Token == token && !s.IsRevoked && s.ExpiresAt > DateTime.UtcNow);
        return sessions.FirstOrDefault();
    }

    public async Task<IEnumerable<Session>> GetActiveSessionsForUserAsync(Guid userId)
    {
        var sessions = await _unitOfWork.GetRepository<Session>().FindAsync(s => s.UserId == userId && !s.IsRevoked && s.ExpiresAt > DateTime.UtcNow);
        return sessions.OrderByDescending(s => s.CreatedAt).ToList();
    }

    public async Task<bool> RevokeSessionAsync(string token, Guid userId)
    {
        _logger.LogInformation("Revoking session token for user: {UserId}", userId);
        var repo = _unitOfWork.GetRepository<Session>();
        var sessions = await repo.FindAsync(s => s.Token == token && s.UserId == userId);
        var session = sessions.FirstOrDefault();
        if (session == null) return false;

        session.IsRevoked = true;
        session.UpdatedAt = DateTime.UtcNow;
        session.UpdatedBy = "system";

        await _unitOfWork.SaveChangesAsync();
        return true;
    }

    public async Task<bool> RevokeAllSessionsForUserExceptCurrentAsync(Guid userId, string currentToken)
    {
        _logger.LogInformation("Revoking all other sessions for user: {UserId}", userId);
        var repo = _unitOfWork.GetRepository<Session>();
        var sessions = await repo.FindAsync(s => s.UserId == userId && s.Token != currentToken && !s.IsRevoked);

        foreach (var session in sessions)
        {
            session.IsRevoked = true;
            session.UpdatedAt = DateTime.UtcNow;
        }

        await _unitOfWork.SaveChangesAsync();
        return true;
    }

    public async Task<bool> RevokeAllSessionsForUserAsync(Guid userId)
    {
        _logger.LogInformation("Revoking all sessions for user: {UserId}", userId);
        var repo = _unitOfWork.GetRepository<Session>();
        var sessions = await repo.FindAsync(s => s.UserId == userId && !s.IsRevoked);

        foreach (var session in sessions)
        {
            session.IsRevoked = true;
            session.UpdatedAt = DateTime.UtcNow;
        }

        await _unitOfWork.SaveChangesAsync();
        return true;
    }
}
