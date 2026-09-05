using System;
using System.Threading.Tasks;
using BusinessManagement.Shared.Security;
using Xunit;

namespace BusinessManagement.Tests.Security;

public class TotpHelperTests
{
    [Fact]
    public void GenerateSecretKey_ShouldReturnValidBase32String()
    {
        // Act
        string secret = TotpHelper.GenerateSecretKey();

        // Assert
        Assert.NotNull(secret);
        Assert.NotEmpty(secret);
        Assert.Matches(@"^[A-Z2-7]+$", secret); // Base32 alphabet check
    }

    [Fact]
    public void GenerateCodeAndVerify_ShouldBeSuccessful()
    {
        // Arrange
        string secret = TotpHelper.GenerateSecretKey();

        // Act
        string code = TotpHelper.GenerateCode(secret);
        bool isValid = TotpHelper.VerifyCode(secret, code);

        // Assert
        Assert.NotNull(code);
        Assert.Equal(6, code.Length);
        Assert.True(isValid);
    }

    [Fact]
    public void VerifyCode_WithInvalidCode_ShouldReturnFalse()
    {
        // Arrange
        string secret = TotpHelper.GenerateSecretKey();

        // Act
        bool isValid = TotpHelper.VerifyCode(secret, "999999");

        // Assert
        Assert.False(isValid);
    }
}
