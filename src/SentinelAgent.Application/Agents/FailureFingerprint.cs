using System.Security.Cryptography;
using System.Text;

namespace SentinelAgent.Application.Agents;

public static class FailureFingerprint
{
    public static string Generate(string input)
    {
        var normalized = Normalize(input);
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(normalized));
        return Convert.ToHexString(hash);
    }

    private static string Normalize(string input)
    {
        return input
            .ToLowerInvariant()
            .Replace("\r", "")
            .Replace("\n", "")
            .Trim();
    }
}