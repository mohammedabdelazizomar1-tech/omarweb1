using System;
using BusinessManagement.Shared.Security;
using Xunit;

namespace BusinessManagement.Tests.Security;

public class EncryptionHelperTests
{
    [Fact]
    public void EncryptAndDecrypt_ShouldReturnOriginalText()
    {
        // Arrange
        string originalText = "SensitiveData123!@#";

        // Act
        string cipherText = EncryptionHelper.Encrypt(originalText);
        string decryptedText = EncryptionHelper.Decrypt(cipherText);

        // Assert
        Assert.NotNull(cipherText);
        Assert.StartsWith("v1:", cipherText); // Assert version metadata prefix
        Assert.NotEqual(originalText, cipherText);
        Assert.Equal(originalText, decryptedText);
    }

    [Fact]
    public void Decrypt_WithInvalidOrLegacyPlaintext_ShouldReturnInputString()
    {
        // Arrange
        string plainText = "LegacyUnencryptedData";

        // Act
        string result = EncryptionHelper.Decrypt(plainText);

        // Assert
        Assert.Equal(plainText, result);
    }

    [Fact]
    public void HashSearchableField_ShouldBeDeterministicAndDifferentFromInput()
    {
        // Arrange
        string input = "AB1234567";

        // Act
        string hash1 = EncryptionHelper.HashSearchableField(input);
        string hash2 = EncryptionHelper.HashSearchableField(input);
        string hash3 = EncryptionHelper.HashSearchableField("ab1234567 "); // test normalization

        // Assert
        Assert.NotNull(hash1);
        Assert.StartsWith("pv1:", hash1); // Assert pepper version prefix
        Assert.Equal(hash1, hash2);
        Assert.Equal(hash1, hash3); // Assert case and spacing normalization
        Assert.NotEqual(input, hash1);
    }
}
