using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BusinessManagement.Application.Interfaces;
using BusinessManagement.Application.Services;
using BusinessManagement.Domain.Entities;
using BusinessManagement.Domain.Repositories;
using BusinessManagement.Shared.Security;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace BusinessManagement.Tests.Services;

public class IdentityServiceTests
{
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<IRepository<User>> _mockUserRepo;
    private readonly Mock<IRepository<UserPasswordHistory>> _mockHistoryRepo;
    private readonly Mock<IPasswordHasher> _mockPasswordHasher;
    private readonly Mock<ISecurityAuditService> _mockAuditService;
    private readonly Mock<ILogger<IdentityService>> _mockLogger;
    private readonly IdentityService _identityService;

    public IdentityServiceTests()
    {
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockUserRepo = new Mock<IRepository<User>>();
        _mockHistoryRepo = new Mock<IRepository<UserPasswordHistory>>();
        _mockPasswordHasher = new Mock<IPasswordHasher>();
        _mockAuditService = new Mock<ISecurityAuditService>();
        _mockLogger = new Mock<ILogger<IdentityService>>();

        _mockUnitOfWork.Setup(u => u.GetRepository<User>()).Returns(_mockUserRepo.Object);
        _mockUnitOfWork.Setup(u => u.GetRepository<UserPasswordHistory>()).Returns(_mockHistoryRepo.Object);

        _identityService = new IdentityService(
            _mockUnitOfWork.Object,
            _mockPasswordHasher.Object,
            _mockAuditService.Object,
            _mockLogger.Object
        );
    }

    [Fact]
    public async Task AuthenticateAsync_WithUnverifiedEmail_ReturnsUnconfirmedDto()
    {
        // Arrange
        var user = new User
        {
            Id = Guid.NewGuid(),
            Username = "employee1",
            Email = "emp1@agency.com",
            PasswordHash = "hashed_pw",
            EmailConfirmed = false, // unverified!
            IsActive = true
        };

        _mockUserRepo.Setup(r => r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<User, bool>>>(), It.IsAny<bool>()))
            .ReturnsAsync(new List<User> { user });

        _mockPasswordHasher.Setup(h => h.VerifyPassword("Pass123!", "hashed_pw"))
            .Returns(true);

        // Act
        var result = await _identityService.AuthenticateAsync("employee1", "Pass123!", "127.0.0.1", "Chrome");

        // Assert
        Assert.NotNull(result);
        Assert.False(result.EmailConfirmed); // EmailConfirmed is false in the DTO, blocking session creation
    }

    [Fact]
    public async Task ChangePassword_WithReusedRecentPassword_ThrowsException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new User
        {
            Id = userId,
            Username = "employee1",
            PasswordHash = "hash_current"
        };

        _mockUserRepo.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync(user);
        _mockPasswordHasher.Setup(h => h.VerifyPassword("CurrentPass1!", "hash_current")).Returns(true);

        // Simulate password history containing "hash_old"
        var history = new List<UserPasswordHistory>
        {
            new UserPasswordHistory { UserId = userId, PasswordHash = "hash_old", CreatedAt = DateTime.UtcNow }
        };
        _mockHistoryRepo.Setup(r => r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<UserPasswordHistory, bool>>>()))
            .ReturnsAsync(history);

        // Simulate new password matches the historical password
        _mockPasswordHasher.Setup(h => h.VerifyPassword("ReusedPass1!", "hash_old")).Returns(true);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => 
            _identityService.ChangePasswordAsync(userId, "CurrentPass1!", "ReusedPass1!")
        );
    }

    [Fact]
    public async Task ChangePassword_WithInvalidComplexity_ThrowsException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new User
        {
            Id = userId,
            Username = "employee1",
            PasswordHash = "hash_current"
        };

        _mockUserRepo.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync(user);
        _mockPasswordHasher.Setup(h => h.VerifyPassword("CurrentPass1!", "hash_current")).Returns(true);
        _mockHistoryRepo.Setup(r => r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<UserPasswordHistory, bool>>>()))
            .ReturnsAsync(new List<UserPasswordHistory>());

        // Act & Assert: try setting password with missing special character or lowercase
        await Assert.ThrowsAsync<InvalidOperationException>(() => 
            _identityService.ChangePasswordAsync(userId, "CurrentPass1!", "simple1")
        );
    }
}
