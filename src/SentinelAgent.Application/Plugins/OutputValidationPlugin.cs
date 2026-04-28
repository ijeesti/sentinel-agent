using Microsoft.SemanticKernel;
using System.ComponentModel;

namespace SentinelAgent.Application.Plugins;

public class OutputValidationPlugin
{
    [KernelFunction, Description("Validates the AI output for professional tone and formatting.")]
    public string ValidateIncidentReport(string aiGeneratedHtml)
    {
        // Check for "Hallucinations" or unprofessional language
        string[] forbiddenWords = { "I think", "maybe", "probably", "oops" };

        foreach (var word in forbiddenWords)
        {
            if (aiGeneratedHtml.Contains(word, StringComparison.OrdinalIgnoreCase))
            {
                // Logic to flag for human review or re-prompting
                return "AI output flagged for manual review: Low confidence language detected.";
            }
        }

        // Ensure the disclaimer is always present for legal safety
        if (!aiGeneratedHtml.Contains("AI-Generated Analysis"))
        {
            aiGeneratedHtml += "<p><i>Disclaimer: AI-Generated Analysis. Verify details before action.</i></p>";
        }

        return aiGeneratedHtml;
    }
}
