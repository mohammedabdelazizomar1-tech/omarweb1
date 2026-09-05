using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using BusinessManagement.Domain.Entities;
using BusinessManagement.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BusinessManagement.Persistence.Services;

public class AuditIntegrityVerifier
{
    private readonly BusinessManagementDbContext _dbContext;
    private readonly ILogger<AuditIntegrityVerifier> _logger;

    public AuditIntegrityVerifier(BusinessManagementDbContext dbContext, ILogger<AuditIntegrityVerifier> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<bool> VerifyChainAsync(bool verifyAll = false)
    {
        _logger.LogInformation("Starting Audit Ledger Chain Integrity Verification. VerifyAll: {VerifyAll}", verifyAll);

        // Retrieve audit key
        string auditKey = Environment.GetEnvironmentVariable("APP_AUDIT_LEDGER_KEY") ?? "SecureDefaultAuditLedgerKey123!@#";
        byte[] keyBytes = Encoding.UTF8.GetBytes(auditKey);

        // Get last verified log creation date checkpoint
        DateTime? checkpointDate = null;
        if (!verifyAll)
        {
            var checkpointSetting = await _dbContext.Settings
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(s => s.Key == "AuditVerificationCheckpoint");
            if (checkpointSetting != null && DateTime.TryParse(checkpointSetting.Value, out var date))
            {
                checkpointDate = date;
            }
        }

        // Query logs to verify
        var query = _dbContext.AuditLogs.IgnoreQueryFilters().OrderBy(a => a.CreatedAt);
        var logs = checkpointDate.HasValue 
            ? await query.Where(a => a.CreatedAt >= checkpointDate.Value).ToListAsync()
            : await query.ToListAsync();

        if (!logs.Any())
        {
            _logger.LogInformation("No audit log records found to verify.");
            return true;
        }

        bool isIntact = true;
        string? previousHash = logs[0].PreviousLogHash;

        foreach (var log in logs)
        {
            // Validate previous hash matching
            if (log.PreviousLogHash != previousHash)
            {
                _logger.LogCritical("AUDIT LEDGER TAMPERING DETECTED! Broken chain at AuditLog ID {LogId}. Expected PreviousHash: {Expected}, Actual: {Actual}", log.Id, previousHash, log.PreviousLogHash);
                isIntact = false;
                break;
            }

            // Recalculate HMAC-SHA256
            string payload = $"{log.PreviousLogHash}|{log.CreatedAt:o}|{log.UserId}|{log.EntityName}|{log.EntityId}|{log.NewValues}";
            using (var hmac = new HMACSHA256(keyBytes))
            {
                byte[] hashBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(payload));
                string computedHash = Convert.ToHexString(hashBytes).ToLowerInvariant();

                if (log.LogHash != computedHash)
                {
                    _logger.LogCritical("AUDIT LEDGER TAMPERING DETECTED! Recalculated hash mismatch at AuditLog ID {LogId}. Expected: {Expected}, Actual: {Actual}", log.Id, computedHash, log.LogHash);
                    isIntact = false;
                    break;
                }
            }

            previousHash = log.LogHash;
        }

        if (isIntact)
        {
            _logger.LogInformation("Audit Ledger integrity verified successfully. Verified {Count} entries.", logs.Count);
            
            // Update checkpoint setting to the last verified log timestamp
            var lastLog = logs.Last();
            var checkpointSetting = await _dbContext.Settings
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(s => s.Key == "AuditVerificationCheckpoint");
            if (checkpointSetting == null)
            {
                checkpointSetting = new Setting { Key = "AuditVerificationCheckpoint", Value = lastLog.CreatedAt.ToString("o") };
                _dbContext.Settings.Add(checkpointSetting);
            }
            else
            {
                checkpointSetting.Value = lastLog.CreatedAt.ToString("o");
            }
            await _dbContext.SaveChangesAsync();
        }

        return isIntact;
    }
}
