using System;
using System.Security.Cryptography;

namespace BusinessManagement.Shared.Security;

public class Pbkdf2PasswordHasher : IPasswordHasher
{
    private const int SaltSize = 16; // 128 bit
    private const int KeySize = 32;  // 256 bit
    private const int Iterations = 100000;
    private static readonly HashAlgorithmName HashAlgorithm = HashAlgorithmName.SHA256;

    public string HashPassword(string password)
    {
        if (password == null) throw new ArgumentNullException(nameof(password));

        byte[] salt = RandomNumberGenerator.GetBytes(SaltSize);
        byte[] hash = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, HashAlgorithm, KeySize);

        return $"{Iterations}.{Convert.ToBase64String(salt)}.{Convert.ToBase64String(hash)}";
    }

    public bool VerifyPassword(string password, string passwordHash)
    {
        if (password == null) throw new ArgumentNullException(nameof(password));
        if (passwordHash == null) throw new ArgumentNullException(nameof(passwordHash));

        string[] parts = passwordHash.Split('.', 3);
        if (parts.Length != 3)
        {
            return false;
        }

        if (!int.TryParse(parts[0], out int iterations))
        {
            return false;
        }

        byte[] salt = Convert.FromBase64String(parts[1]);
        byte[] hash = Convert.FromBase64String(parts[2]);

        byte[] testHash = Rfc2898DeriveBytes.Pbkdf2(password, salt, iterations, HashAlgorithm, hash.Length);

        return CryptographicOperations.FixedTimeEquals(hash, testHash);
    }
}

