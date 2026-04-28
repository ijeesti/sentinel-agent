using Microsoft.SemanticKernel;
using System.ComponentModel;
using System.Text.RegularExpressions;

namespace SentinelAgent.Application.Plugins;

public class SecurityGuardrailPlugin
{
    [KernelFunction, Description("Cleans PII and secrets from stack traces.")]
    public string Sanitize(string rawStackTrace)
    {
        if (string.IsNullOrEmpty(rawStackTrace)) return rawStackTrace;

        // Use a single variable to accumulate changes
        string result = rawStackTrace;

        // 1. Mask Potential IP Addresses
        string ipPattern = @"\b\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}\b";
        result = Regex.Replace(result, ipPattern, "[IP_REDACTED]");

        // 2. Mask Connection Strings / Credentials
        // Added (?i) for case-insensitivity and handled whitespace better
        string secretPattern = @"(?i)(password|pwd|secret|token|key|bearer)\s*[:=]\s*[^;\s]+";
        result = Regex.Replace(result, secretPattern, "$1=********");

        // 3. Mask Emails
        // Added RegexOptions.IgnoreCase to ensure JOHN.DOE@... is caught
        string emailPattern = @"[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}";
        result = Regex.Replace(result, emailPattern, "[EMAIL_REDACTED]", RegexOptions.IgnoreCase);

        return result;
    }
}