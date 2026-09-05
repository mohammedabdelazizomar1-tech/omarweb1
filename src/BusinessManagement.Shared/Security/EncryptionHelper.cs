using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace BusinessManagement.Shared.Security;

public static class EncryptionHelper
{
    private static readonly byte[] MasterKey;
    private static readonly byte[] PepperBytes;
    private const string CurrentKeyVersion = "v1";

    static EncryptionHelper()
    {
        // Secret Management
        var envKey = Environment.GetEnvironmentVariable("APP_ENCRYPTION_KEY");
        var envPepper = Environment.GetEnvironmentVariable("APP_SEARCH_PEPPER");

        if (!string.IsNullOrEmpty(envKey))
        {
            try
            {
                MasterKey = Convert.FromBase64String(envKey);
            }
            catch
            {
                MasterKey = SHA256.HashData(Encoding.UTF8.GetBytes(envKey));
            }
        }
        else
        {
            // Development fallback master key (exactly 32 bytes)
            MasterKey = SHA256.HashData(Encoding.UTF8.GetBytes("BusinessManagementEnterpriseSecureKey2026MasterKey"));
        }

        if (!string.IsNullOrEmpty(envPepper))
        {
            PepperBytes = Encoding.UTF8.GetBytes(envPepper);
        }
        else
        {
            // Development fallback pepper
            PepperBytes = Encoding.UTF8.GetBytes("BusinessManagementEnterpriseSecurePepper2026");
        }
    }

    public static string Encrypt(string plainText)
    {
        if (string.IsNullOrEmpty(plainText)) return plainText;

        byte[] plainBytes = Encoding.UTF8.GetBytes(plainText);
        
        // GCM standard nonce size is 12 bytes
        byte[] nonce = RandomNumberGenerator.GetBytes(12);
        // GCM standard tag size is 16 bytes
        byte[] tag = new byte[16];
        byte[] cipherBytes = new byte[plainBytes.Length];

        using var aesGcm = new AesGcm(MasterKey, tag.Length);
        aesGcm.Encrypt(nonce, plainBytes, cipherBytes, tag);

        // Package as: KeyVersionPrefix + Base64(Nonce + Tag + CipherBytes)
        // e.g. "v1:Base64String"
        byte[] resultBytes = new byte[nonce.Length + tag.Length + cipherBytes.Length];
        Buffer.BlockCopy(nonce, 0, resultBytes, 0, nonce.Length);
        Buffer.BlockCopy(tag, 0, resultBytes, nonce.Length, tag.Length);
        Buffer.BlockCopy(cipherBytes, 0, resultBytes, nonce.Length + tag.Length, cipherBytes.Length);

        return $"{CurrentKeyVersion}:{Convert.ToBase64String(resultBytes)}";
    }

    public static string Decrypt(string cipherTextWithMeta)
    {
        if (string.IsNullOrEmpty(cipherTextWithMeta)) return cipherTextWithMeta;

        try
        {
            // Parse KeyVersion
            int colonIndex = cipherTextWithMeta.IndexOf(':');
            if (colonIndex < 0)
            {
                // Fallback for unversioned values or legacy plaintext
                return cipherTextWithMeta;
            }

            string version = cipherTextWithMeta[..colonIndex];
            string base64Payload = cipherTextWithMeta[(colonIndex + 1)..];
            byte[] payload = Convert.FromBase64String(base64Payload);

            // Extract Nonce (12 bytes), Tag (16 bytes), and CipherBytes (rest)
            const int nonceSize = 12;
            const int tagSize = 16;

            if (payload.Length < nonceSize + tagSize)
            {
                return cipherTextWithMeta;
            }

            byte[] nonce = new byte[nonceSize];
            byte[] tag = new byte[tagSize];
            byte[] cipherBytes = new byte[payload.Length - nonceSize - tagSize];

            Buffer.BlockCopy(payload, 0, nonce, 0, nonceSize);
            Buffer.BlockCopy(payload, nonceSize, tag, 0, tagSize);
            Buffer.BlockCopy(payload, nonceSize + tagSize, cipherBytes, 0, cipherBytes.Length);

            byte[] plainBytes = new byte[cipherBytes.Length];

            // In production, we'd load the correct key based on 'version'
            // For now, we decrypt using current MasterKey
            using var aesGcm = new AesGcm(MasterKey, tagSize);
            aesGcm.Decrypt(nonce, cipherBytes, tag, plainBytes);

            return Encoding.UTF8.GetString(plainBytes);
        }
        catch
        {
            // Return raw string if decryption fails (safeguard for non-migrated/legacy data)
            return cipherTextWithMeta;
        }
    }

    public static string HashSearchableField(string val)
    {
        if (string.IsNullOrWhiteSpace(val)) return string.Empty;

        // Normalize value (uppercase and trimmed)
        string normalized = val.Trim().ToUpperInvariant();
        byte[] inputBytes = Encoding.UTF8.GetBytes(normalized);

        // Implement HMAC-SHA256 for searchable lookup
        using var hmac = new HMACSHA256(PepperBytes);
        byte[] hashBytes = hmac.ComputeHash(inputBytes);

        // Prepend PepperVersion (e.g. "pv1:") for Pepper rotation support
        return $"pv1:{Convert.ToBase64String(hashBytes)}";
    }
}
