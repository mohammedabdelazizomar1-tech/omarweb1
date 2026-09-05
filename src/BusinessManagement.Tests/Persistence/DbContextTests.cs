using System;
using System.Linq;
using System.Threading.Tasks;
using BusinessManagement.Domain;
using BusinessManagement.Domain.Entities;
using BusinessManagement.Persistence;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace BusinessManagement.Tests.Persistence;

public class DbContextTests
{
    private readonly Mock<ICurrentUserProvider> _mockUserProvider;
    private readonly DbContextOptions<BusinessManagementDbContext> _dbOptions;

    public DbContextTests()
    {
        _mockUserProvider = new Mock<ICurrentUserProvider>();
        _dbOptions = new DbContextOptionsBuilder<BusinessManagementDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
    }

    [Fact]
    public async Task TenantIsolation_GlobalQueryFilter_ShouldFilterRecords()
    {
        // Arrange
        var tenantId1 = Guid.NewGuid();
        var tenantId2 = Guid.NewGuid();

        _mockUserProvider.Setup(u => u.GetCurrentTenantId()).Returns(tenantId1);
        _mockUserProvider.Setup(u => u.GetCurrentUsername()).Returns("test-user");

        using (var context = new BusinessManagementDbContext(_dbOptions, _mockUserProvider.Object))
        {
            // Seed passenger records for different tenants
            context.Passengers.Add(new Passenger
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId1,
                FullName = "Tenant 1 Passenger",
                PassportNumber = "PASS1",
                Nationality = "Egyptian"
            });

            context.Passengers.Add(new Passenger
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId2,
                FullName = "Tenant 2 Passenger",
                PassportNumber = "PASS2",
                Nationality = "Egyptian"
            });

            await context.SaveChangesAsync();
        }

        // Act & Assert: Load context with Tenant 1 provider
        using (var context = new BusinessManagementDbContext(_dbOptions, _mockUserProvider.Object))
        {
            var passengers = await context.Passengers.ToListAsync();

            // Verify only Tenant 1 records are returned (Tenant Isolation is working!)
            Assert.Single(passengers);
            Assert.Equal("Tenant 1 Passenger", passengers[0].FullName);
        }
    }

    [Fact]
    public async Task SaveChangesAsync_WithAuditLogUpdateOrDelete_ShouldThrowException()
    {
        // Arrange
        _mockUserProvider.Setup(u => u.GetCurrentUsername()).Returns("system");
        
        using var context = new BusinessManagementDbContext(_dbOptions, _mockUserProvider.Object);
        var audit = new AuditLog
        {
            Id = Guid.NewGuid(),
            EntityName = "TestEntity",
            EntityId = Guid.NewGuid(),
            UserId = "system",
            CreatedAt = DateTime.UtcNow
        };
        context.AuditLogs.Add(audit);
        await context.SaveChangesAsync();

        // Act & Assert 1: Prevent updates
        audit.EntityName = "TamperedEntityName";
        await Assert.ThrowsAsync<InvalidOperationException>(() => context.SaveChangesAsync());

        // Act & Assert 2: Prevent deletions
        context.Entry(audit).State = EntityState.Unchanged; // Reset state
        context.AuditLogs.Remove(audit);
        await Assert.ThrowsAsync<InvalidOperationException>(() => context.SaveChangesAsync());
    }

    [Fact]
    public async Task SoftDelete_GlobalQueryFilter_ShouldFilterDeletedRecords()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        _mockUserProvider.Setup(u => u.GetCurrentTenantId()).Returns(tenantId);
        _mockUserProvider.Setup(u => u.GetCurrentUsername()).Returns("test-user");

        var passengerId = Guid.NewGuid();

        using (var context = new BusinessManagementDbContext(_dbOptions, _mockUserProvider.Object))
        {
            var passenger = new Passenger
            {
                Id = passengerId,
                TenantId = tenantId,
                FullName = "To be deleted",
                PassportNumber = "DEL1",
                Nationality = "Egyptian"
            };
            context.Passengers.Add(passenger);
            await context.SaveChangesAsync();
        }

        // Act: Delete passenger
        using (var context = new BusinessManagementDbContext(_dbOptions, _mockUserProvider.Object))
        {
            var passenger = await context.Passengers.FindAsync(passengerId);
            Assert.NotNull(passenger);
            context.Passengers.Remove(passenger);
            await context.SaveChangesAsync();
        }

        // Assert: Verify it's soft deleted (IsDeleted = true in DB, but not returned by query)
        using (var context = new BusinessManagementDbContext(_dbOptions, _mockUserProvider.Object))
        {
            var activePassengers = await context.Passengers.ToListAsync();
            Assert.Empty(activePassengers); // Hidden by global filter

            // Fetch via IgnoreQueryFilters to verify soft delete columns are updated
            var deletedPassenger = await context.Passengers
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(p => p.Id == passengerId);

            Assert.NotNull(deletedPassenger);
            Assert.True(deletedPassenger.IsDeleted);
            Assert.Equal("test-user", deletedPassenger.DeletedBy);
        }
    }

    [Fact]
    public async Task SaveChangesAsync_WithEmptyTenantId_ShouldAutoAssignTenantId()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        _mockUserProvider.Setup(u => u.GetCurrentTenantId()).Returns(tenantId);
        _mockUserProvider.Setup(u => u.GetCurrentUsername()).Returns("test-user");

        using (var context = new BusinessManagementDbContext(_dbOptions, _mockUserProvider.Object))
        {
            var tx = new SafeTransaction
            {
                Amount = 1000m,
                TransactionType = Domain.Enums.TransactionType.Deposit,
                Currency = Domain.Enums.Currency.EGP,
                DepositorOrWithdrawerName = "Auto-Tenant Test",
                TransactionDate = DateOnly.FromDateTime(DateTime.UtcNow)
            };
            context.SafeTransactions.Add(tx);
            await context.SaveChangesAsync();

            // Act & Assert
            var savedTx = await context.SafeTransactions.IgnoreQueryFilters().FirstOrDefaultAsync(t => t.Id == tx.Id);
            Assert.NotNull(savedTx);
            Assert.Equal(tenantId, savedTx.TenantId);
        }
    }
}
