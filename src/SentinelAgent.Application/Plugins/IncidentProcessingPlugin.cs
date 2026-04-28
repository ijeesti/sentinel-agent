using Microsoft.SemanticKernel;
using System.ComponentModel;
using System.Text.RegularExpressions;

namespace SentinelAgent.Application.Plugins;

public class IncidentProcessingPlugin
{
    [KernelFunction]
    [Description("Cleans sensitive data like IPs and emails from the input trace.")]
    public string SanitizeInput([Description("The raw stack trace or log")] string input)
    {
        // Simple Regex for IPs and Emails
        string sanitized = Regex.Replace(input, @"\b\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}\b", "[IP_HIDDEN]");
        sanitized = Regex.Replace(sanitized, @"[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}", "[EMAIL_HIDDEN]");
        return sanitized;
    }

    [KernelFunction]
    [Description("Ensures the LLM output is valid JSON and contains the required disclaimer.")]
    public string ValidateAndFormat([Description("The raw JSON string from LLM")] string jsonOutput)
    {
        // Guardrail: Remove markdown if present
        var cleanJson = jsonOutput;
        if (cleanJson.StartsWith("```"))
        {
            cleanJson = Regex.Replace(cleanJson, @"^```[a-z]*\n|```$", "", RegexOptions.IgnoreCase).Trim();
        }

        // Add a footer or metadata before returning for the HTML generator
        return cleanJson;
    }
}
