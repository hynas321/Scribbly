using System.Security.Cryptography;
using System.Text;

namespace Scribbly.WebApi.Auth;

public static class SecretComparer
{
    public static bool FixedTimeEquals(string? supplied, string? expected)
    {
        if (string.IsNullOrWhiteSpace(supplied) || string.IsNullOrWhiteSpace(expected))
        {
            return false;
        }
        byte[] left = SHA256.HashData(Encoding.UTF8.GetBytes(supplied));
        byte[] right = SHA256.HashData(Encoding.UTF8.GetBytes(expected));
        return CryptographicOperations.FixedTimeEquals(left, right);
    }
}
