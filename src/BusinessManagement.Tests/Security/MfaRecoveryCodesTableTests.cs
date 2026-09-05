using System;
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

namespace BusinessManagement.Tests.Security;

public class MfaRecoveryCodesTableTests
{
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<IRepository<User>> _mockUserRepo;
    private readonly Mock<IRepository<UserMfaRecoveryCode>> _mockRecoveryRepo;
    private readonly Mock<IPasswordHasher> _mockPasswordHasher;
    private readonly Mock<ISecurityAuditService> _mockAuditService;
    private readonly Mock<ILogger<IdentityService>> _mockLogger;
    private readonly IdentityService _identityService;

    public MfaRecoveryCodesTableTests()
    {
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockUserRepo = new Mock<IRepository<User>>();
        _mockRecoveryRepo = new Mock<IRepository<UserMfaRecoveryCode>>();
        _mockPasswordHasher = new Mock<IPasswordHasher>();
        _mockAuditService = new Mock<ISecurityAuditService>();
        _mockLogger = new Mock<ILogger<IdentityService>>();

        _mockUnitOfWork.Setup(u => u.GetRepository<User>()).Returns(_mockUserRepo.Object);
        _mockUnitOfWork.Setup(u => u.GetRepository<UserMfaRecoveryCode>()).Returns(_mockRecoveryRepo.Object);

        _identityService = new IdentityService(
            _mockUnitOfWork.Object,
            _mockPasswordHasher.Object,
            _mockAuditService.Object,
            _mockLogger.Object
        );
    }

    [Fact]
    public async Task GenerateMfaRecoveryCodesAsync_ShouldCreateAndSaveFiveCodes()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new User { Id = userId, Username = "admin" };
        _mockUserRepo.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync(user);

        // Capture added recovery code entities
        var addedCodes = new System.Collections.Generic.List<UserMfaRecoveryCode>();
        _mockRecoveryRepo.Setup(r => r.AddAsync(It.IsAny<UserMfaRecoveryCode>()))
            .Callback<UserMfaRecoveryCode>(c => addedCodes.Add(c))
            .Returns(Task.CompletedTask);

        _mockRecoveryRepo.Setup(r => r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<UserMfaRecoveryCode, bool>>>()))
            .ReturnsAsync(new System.Collections.Generic.List<UserMfaRecoveryCode>());

        // Act
        var codes = await _identityService.GenerateMfaRecoveryCodesAsync(userId);

        // Assert
        Assert.NotNull(codes);
        Assert.Equal(5, codes.Count());
        Assert.Equal(5, addedCodes.Count);
        Assert.All(addedCodes, c => Assert.Equal(userId, c.UserId));
        Assert.All(addedCodes, c => Assert.NotNull(c.CodeHash));
    }

    [Fact]
    public async Task VerifyMfaRecoveryCodeAsync_WithValidUnusedCode_ShouldConsumeAndReturnTrue()
    {
        // Arrange
        var userId = Guid.NewGuid();
        string plainCode = "ABCDEFGH";
        
        // Setup Hash mapping matching the code
        string codeHash = "test_hash_value";
        
        // Mock HashRecoveryCode behavior
        // Since HashRecoveryCode uses HMAC, we'll configure FindAsync to match whatever predicate is executed
        var matchingCode = new UserMfaRecoveryCode
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            CodeHash = codeHash,
            ConsumedAt = null
        };

        _mockRecoveryRepo.Setup(r => r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<UserMfaRecoveryCode, bool>>>()))
            .ReturnsAsync(new System.Collections.Generic.List<UserMfaRecoveryCode> { matchingCode });

        // Act
        bool result = await _identityService.VerifyMfaRecoveryCodeAsync(userId, plainCode);

        // Assert
        Assert.True(result);
        Assert.NotNull(matchingCode.ConsumedAt); // Must be marked as consumed!
        _mockRecoveryRepo.Verify(r => r.Update(matchingCode), Times.Once);
    }
}
