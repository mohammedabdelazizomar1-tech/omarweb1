using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BusinessManagement.Application.Services;
using BusinessManagement.Domain.Entities;
using BusinessManagement.Domain.Repositories;
using BusinessManagement.Shared.Security;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace BusinessManagement.Tests.Services;

public class UserServiceTests
{
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<IRepository<User>> _mockUserRepository;
    private readonly Mock<IPasswordHasher> _mockPasswordHasher;
    private readonly Mock<ILogger<UserService>> _mockLogger;
    private readonly UserService _userService;

    public UserServiceTests()
    {
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockUserRepository = new Mock<IRepository<User>>();
        _mockPasswordHasher = new Mock<IPasswordHasher>();
        _mockLogger = new Mock<ILogger<UserService>>();

        _mockUnitOfWork.Setup(u => u.GetRepository<User>()).Returns(_mockUserRepository.Object);

        _userService = new UserService(
            _mockUnitOfWork.Object,
            _mockPasswordHasher.Object,
            _mockLogger.Object
        );
    }

    [Fact]
    public async Task LoginAsync_WithInvalidUsername_ReturnsNull()
    {
        // Arrange
        _mockUserRepository.Setup(r => r.FindAsync(
            It.IsAny<System.Linq.Expressions.Expression<Func<User, bool>>>(),
            It.IsAny<bool>()
        )).ReturnsAsync(new List<User>()); // Return empty list to simulate user not found

        // Act
        var result = await _userService.LoginAsync("nonexistent", "password");

        // Assert
        Assert.Null(result);
    }
}
