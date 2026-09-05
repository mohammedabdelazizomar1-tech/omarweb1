using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using BusinessManagement.Domain.Entities;
using BusinessManagement.Persistence;
using BusinessManagement.Persistence.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace BusinessManagement.Tests.Security;

public class AuditLedgerHMACChainTests
{
    private readonly DbContextOptions<BusinessManagementDbContext> _dbOptions;

    public AuditLedgerHMACChainTests()
    {
        _dbOptions = new DbContextOptionsBuilder<BusinessManagementDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
    }

    [Fact]
    public async Task SaveChangesAsync_ShouldGenerateValidHMACLedgerChain()
    {
        // Arrange
        var mockUserProvider = new Mock<BusinessManagement.Domain.ICurrentUserProvider>();
        mockUserProvider.Setup(u => u.GetCurrentUsername()).Returns("test-operator");
        mockUserProvider.Setup(u => u.GetCurrentTenantId()).Returns(Guid.NewGuid());

        string auditKey = "TestLedgerSecretKeyHex123!@#$";
        Environment.SetEnvironmentVariable("APP_AUDIT_LEDGER_KEY", auditKey);

        using (var context = new BusinessManagementDbContext(_dbOptions, mockUserProvider.Object))
        {
            var passenger = new Passenger
            {
                Id = Guid.NewGuid(),
                FullName = "Original Name",
                PassportNumber = "PASS100",
                Nationality = "Egyptian"
            };

            context.Passengers.Add(passenger);
            await context.SaveChangesAsync();

            // Act: Modify the entity to trigger AuditLog generation (which triggers on Modified state)
            passenger.FullName = "Modified Name";
            context.Passengers.Update(passenger);
            await context.SaveChangesAsync();
        }

        // Verify the generated audit ledger entries
        using (var context = new BusinessManagementDbContext(_dbOptions, mockUserProvider.Object))
        {
            var logs = await context.AuditLogs.ToListAsync();
            Assert.NotEmpty(logs);

            var firstLog = logs.First();
            Assert.NotNull(firstLog.LogHash);
            Assert.Equal("0000000000000000000000000000000000000000000000000000000000000000", firstLog.PreviousLogHash);

            // Recalculate HMAC hash to verify signature
            string payload = $"{firstLog.PreviousLogHash}|{firstLog.CreatedAt:o}|{firstLog.UserId}|{firstLog.EntityName}|{firstLog.EntityId}|{firstLog.NewValues}";
            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(auditKey));
            byte[] hashBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(payload));
            string expectedHash = Convert.ToHexString(hashBytes).ToLowerInvariant();

            Assert.Equal(expectedHash, firstLog.LogHash);
        }
    }

    [Fact]
    public async Task AuditIntegrityVerifier_WithTamperedLogHash_ShouldDetectBrokenChain()
    {
        // Arrange
        var mockUserProvider = new Mock<BusinessManagement.Domain.ICurrentUserProvider>();
        mockUserProvider.Setup(u => u.GetCurrentUsername()).Returns("test-operator");
        mockUserProvider.Setup(u => u.GetCurrentTenantId()).Returns(Guid.NewGuid());

        string auditKey = "TestLedgerSecretKeyHex123!@#$";
        Environment.SetEnvironmentVariable("APP_AUDIT_LEDGER_KEY", auditKey);

        using (var context = new BusinessManagementDbContext(_dbOptions, mockUserProvider.Object))
        {
            // Seed a tampered log directly (with an invalid hash from the start so we don't need to UPDATE it)
            context.AuditLogs.Add(new AuditLog
            {
                Id = Guid.NewGuid(),
                EntityName = "Passenger",
                UserId = "test-operator",
                PreviousLogHash = "0000000000000000000000000000000000000000000000000000000000000000",
                LogHash = "tampered_hash_here", // invalid hash!
                NewValues = "{}"
            });
            await context.SaveChangesAsync();
        }

        // Verify: Run verifier and assert it returns false (chain is broken!)
        using (var context = new BusinessManagementDbContext(_dbOptions, mockUserProvider.Object))
        {
            var mockLogger = new Mock<ILogger<AuditIntegrityVerifier>>();
            var verifier = new AuditIntegrityVerifier(context, mockLogger.Object);

            bool isIntact = await verifier.VerifyChainAsync(verifyAll: true);
            Assert.False(isIntact);
        }
    }
}
