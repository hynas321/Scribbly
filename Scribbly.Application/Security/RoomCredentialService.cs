using System.Security.Cryptography;
using System.Text;
using Scribbly.Application.Abstractions.Security;

namespace Scribbly.Application.Security;

public class RoomCredentialService : IRoomCredentialService
{
    private const string CrockfordAlphabet = "0123456789ABCDEFGHJKMNPQRSTVWXYZ";

    public string GenerateRoomCode()
    {
        Span<byte> bytes = stackalloc byte[10];
        RandomNumberGenerator.Fill(bytes);
        Span<char> output = stackalloc char[16];
        int buffer = 0, bits = 0, index = 0;
        foreach (byte value in bytes)
        {
            buffer = (buffer << 8) | value;
            bits += 8;
            while (bits >= 5)
            {
                bits -= 5;
                output[index++] = CrockfordAlphabet[(buffer >> bits) & 31];
            }
        }
        return new string(output);
    }

    public string GenerateAccessToken()
    {
        Span<byte> bytes = stackalloc byte[32];
        RandomNumberGenerator.Fill(bytes);
        return Convert.ToBase64String(bytes).TrimEnd('=').Replace('+', '-').Replace('/', '_');
    }

    public string ComputeCredentialHash(string accessToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(accessToken);
        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(accessToken)));
    }

    public bool Matches(string accessToken, string hash)
    {
        if (string.IsNullOrWhiteSpace(accessToken) || string.IsNullOrWhiteSpace(hash))
        {
            return false;
        }
        byte[] expected;
        try
        {
            expected = Convert.FromHexString(hash);
        }
        catch (FormatException)
        {
            return false;
        }
        byte[] actual = SHA256.HashData(Encoding.UTF8.GetBytes(accessToken));
        return expected.Length == actual.Length && CryptographicOperations.FixedTimeEquals(actual, expected);
    }
}
