using System;
using System.Security.Cryptography;

namespace BusinessManagement.Shared.Security;

public static class TotpHelper
{
    public static string GenerateSecretKey()
    {
        byte[] buffer = RandomNumberGenerator.GetBytes(10);
        return Base32Encode(buffer);
    }

    public static string GenerateCode(string secretKey)
    {
        byte[] secretBytes = Base32Decode(secretKey);
        long timeStep = DateTime.UtcNow.Ticks / 10000000 / 30; // 30-second window
        byte[] timeStepBytes = BitConverter.GetBytes(timeStep);
        if (BitConverter.IsLittleEndian)
        {
            Array.Reverse(timeStepBytes);
        }

        using var hmac = new HMACSHA1(secretBytes);
        byte[] hash = hmac.ComputeHash(timeStepBytes);

        int offset = hash[^1] & 0x0F;
        int binary = ((hash[offset] & 0x7f) << 24)
                     | ((hash[offset + 1] & 0xff) << 16)
                     | ((hash[offset + 2] & 0xff) << 8)
                     | (hash[offset + 3] & 0xff);

        int otp = binary % 1000000;
        return otp.ToString("D6");
    }

    public static bool VerifyCode(string secretKey, string code, int timeToleranceSeconds = 30)
    {
        if (string.IsNullOrEmpty(secretKey) || string.IsNullOrEmpty(code)) return false;

        try
        {
            byte[] secretBytes = Base32Decode(secretKey);
            long currentTimeStep = DateTime.UtcNow.Ticks / 10000000 / 30;
            int steps = timeToleranceSeconds / 30;

            for (int i = -steps; i <= steps; i++)
            {
                long timeStep = currentTimeStep + i;
                byte[] timeStepBytes = BitConverter.GetBytes(timeStep);
                if (BitConverter.IsLittleEndian)
                {
                    Array.Reverse(timeStepBytes);
                }

                using var hmac = new HMACSHA1(secretBytes);
                byte[] hash = hmac.ComputeHash(timeStepBytes);

                int offset = hash[^1] & 0x0F;
                int binary = ((hash[offset] & 0x7f) << 24)
                             | ((hash[offset + 1] & 0xff) << 16)
                             | ((hash[offset + 2] & 0xff) << 8)
                             | (hash[offset + 3] & 0xff);

                int otp = binary % 1000000;
                if (otp.ToString("D6") == code.Trim())
                {
                    return true;
                }
            }
        }
        catch
        {
            return false;
        }

        return false;
    }

    private static string Base32Encode(byte[] data)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567";
        if (data == null || data.Length == 0) return string.Empty;

        var sb = new System.Text.StringBuilder((data.Length * 8 + 4) / 5);
        int bits = 0;
        int val = 0;
        for (int i = 0; i < data.Length; i++)
        {
            val = (val << 8) | data[i];
            bits += 8;
            while (bits >= 5)
            {
                bits -= 5;
                sb.Append(chars[(val >> bits) & 0x1F]);
            }
        }
        if (bits > 0)
        {
            sb.Append(chars[(val << (5 - bits)) & 0x1F]);
        }
        return sb.ToString();
    }

    private static byte[] Base32Decode(string base32)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567";
        base32 = base32.ToUpperInvariant().TrimEnd('=');
        if (string.IsNullOrEmpty(base32)) return Array.Empty<byte>();

        var result = new List<byte>();
        int bits = 0;
        int val = 0;
        foreach (char c in base32)
        {
            int index = chars.IndexOf(c);
            if (index < 0) continue;

            val = (val << 5) | index;
            bits += 5;
            while (bits >= 8)
            {
                bits -= 8;
                result.Add((byte)((val >> bits) & 0xFF));
            }
        }
        return result.ToArray();
    }
}
