using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using BusinessManagement.Application.DTOs;
using BusinessManagement.Application.Interfaces;
using BusinessManagement.Web.Controllers;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace BusinessManagement.Tests.Controllers;

public class AccountControllerTests
{
    private readonly Mock<IIdentityService> _mockIdentityService;
    private readonly Mock<ISessionService> _mockSessionService;
    private readonly Mock<ISecurityAuditService> _mockAuditService;
    private readonly Mock<IUserService> _mockUserService;
    private readonly Mock<ILogger<AccountController>> _mockLogger;
    private readonly Mock<IAuthenticationService> _mockAuthService;
    private readonly AccountController _controller;

    public AccountControllerTests()
    {
        _mockIdentityService = new Mock<IIdentityService>();
        _mockSessionService = new Mock<ISessionService>();
        _mockAuditService = new Mock<ISecurityAuditService>();
        _mockUserService = new Mock<IUserService>();
        _mockLogger = new Mock<ILogger<AccountController>>();
        _mockAuthService = new Mock<IAuthenticationService>();

        // Set up controller context with mock HTTP context
        var httpContext = new DefaultHttpContext();
        var serviceProvider = new Mock<IServiceProvider>();
        
        // Mock SignInAsync and SignOutAsync
        _mockAuthService.Setup(s => s.SignInAsync(
            It.IsAny<HttpContext>(), It.IsAny<string>(), It.IsAny<ClaimsPrincipal>(), It.IsAny<AuthenticationProperties>()
        )).Returns(Task.CompletedTask);
        
        _mockAuthService.Setup(s => s.SignOutAsync(
            It.IsAny<HttpContext>(), It.IsAny<string>(), It.IsAny<AuthenticationProperties>()
        )).Returns(Task.CompletedTask);

        serviceProvider.Setup(s => s.GetService(typeof(IAuthenticationService))).Returns(_mockAuthService.Object);
        httpContext.RequestServices = serviceProvider.Object;

        _controller = new AccountController(
            _mockIdentityService.Object,
            _mockSessionService.Object,
            _mockAuditService.Object,
            _mockUserService.Object,
            _mockLogger.Object
        )
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            },
            TempData = new TempDataDictionary(httpContext, Mock.Of<ITempDataProvider>()),
            Url = new Mock<IUrlHelper>().Object
        };
    }

    [Fact]
    public async Task Login_POST_WithCorrectCredentialsAndMfaEnabled_RedirectsToVerifyMfa()
    {
        // Arrange
        var loginDto = new LoginRequestDto
        {
            UsernameOrEmail = "admin",
            Password = "Password123!",
            RememberMe = true
        };

        var userDto = new UserDto
        {
            Id = Guid.NewGuid(),
            Username = "admin",
            RequiresMfa = true, // MFA is required
            TwoFactorEnabled = true,
            EmailConfirmed = true
        };

        _mockIdentityService.Setup(s => s.AuthenticateAsync(
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()
        )).ReturnsAsync(userDto);

        // Act
        var result = await _controller.Login(loginDto);

        // Assert


        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("VerifyMFA", redirectResult.ActionName);
        Assert.Equal(userDto.Id.ToString(), _controller.TempData["PreMfaUserId"]);
    }

    [Fact]
    public async Task Login_POST_WithSuccessfulAuth_ClearsExistingSessionAndSetsDeviceCookie()
    {
        // Arrange
        var loginDto = new LoginRequestDto
        {
            UsernameOrEmail = "employee1",
            Password = "Password123!",
            RememberMe = false
        };

        var userDto = new UserDto
        {
            Id = Guid.NewGuid(),
            Username = "employee1",
            RequiresMfa = false, // MFA not enabled
            EmailConfirmed = true
        };

        _mockIdentityService.Setup(s => s.AuthenticateAsync(
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()
        )).ReturnsAsync(userDto);

        // Set up mock SignOutAsync call to verify Session Fixation protection
        _mockAuthService.Setup(s => s.SignOutAsync(
            It.IsAny<HttpContext>(), It.IsAny<string>(), It.IsAny<AuthenticationProperties>()
        )).Returns(Task.CompletedTask).Verifiable("SignOutAsync was not called to prevent Session Fixation!");

        // Act
        var result = await _controller.Login(loginDto);

        // Assert


        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirectResult.ActionName);
        Assert.Equal("Dashboard", redirectResult.ControllerName);

        // Verify session fixation protection called SignOutAsync first
        _mockAuthService.Verify();

        // Verify device-id cookie is generated
        Assert.True(_controller.Response.Headers.ContainsKey("Set-Cookie"));
        var setCookie = _controller.Response.Headers["Set-Cookie"].ToString();
        Assert.Contains("__Host-Device-Id", setCookie);
    }
}
