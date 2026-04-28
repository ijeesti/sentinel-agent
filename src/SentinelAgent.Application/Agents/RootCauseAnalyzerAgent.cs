using SentinelAgent.Application.Plugins;
using SentinelAgent.Application.Prompts;
using SentinelAgent.Domain.Domains;
using SentinelAgent.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using System.Text.Json;

namespace SentinelAgent.Application.Agents;

// <summary>
/// Uses Semantic Kernel + Ollama to determine the root cause of a failure
/// with a structured confidence score and fix suggestion.
/// </summary>
public sealed class RootCauseAnalyzerAgent(
    Kernel kernel,
    ILogger<RootCauseAnalyzerAgent> logger) : IRootCauseAnalyzerAgent
{
    private readonly ILogger<RootCauseAnalyzerAgent> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    public async Task<RootCauseAnalysisResult> AnalyzeAsync(
    AnalyzeFailureRequest request,
    CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        if (!kernel.Plugins.TryGetPlugin(nameof(SecurityGuardrailPlugin), out _))
        {
            kernel.Plugins.AddFromType<SecurityGuardrailPlugin>(nameof(SecurityGuardrailPlugin));
        }
        if (!kernel.Plugins.TryGetPlugin(nameof(OutputValidationPlugin), out _))
        {
            kernel.Plugins.AddFromType<OutputValidationPlugin>(nameof(OutputValidationPlugin));
        }

        var sanitizedResult = await kernel.InvokeAsync(
            nameof(SecurityGuardrailPlugin),
            nameof(SecurityGuardrailPlugin.Sanitize),
            new() { ["rawStackTrace"] = request.RawFailureInput }, cancellationToken);

        var arguments = new KernelArguments
        {
            ["rawInput"] = sanitizedResult.GetValue<string>(),
            ["inputType"] = request.InputType.ToString(),
            ["codeSnippet"] = request.CodeSnippet ?? "No code provided"
        };

        // 2. THE LLM CALL
        var result = await kernel.InvokePromptAsync(
            AgentPrompts.RootCauseAnalyzer,
            arguments,
            cancellationToken: cancellationToken);

        var rawContent = result.GetValue<string>();
        if (string.IsNullOrWhiteSpace(rawContent))
        {
            throw new InvalidOperationException("AI returned an empty response.");
        }
        // 3. POST-LLM GUARDRAIL: Use the imported plugin to clean/extract the JSON
        var cleanJsonInput = await kernel.InvokeAsync<string>
            (nameof(OutputValidationPlugin),
             nameof(OutputValidationPlugin.ValidateIncidentReport), new() 
            { ["aiGeneratedHtml"] = rawContent }
            , cancellationToken);

        if (string.IsNullOrWhiteSpace(cleanJsonInput))
        {
            throw new InvalidOperationException("AI returned an empty response.");
        }

        var cleanJson = CleanJsonString(cleanJsonInput);
        try
        {
            var rca = JsonSerializer.Deserialize<RootCauseAnalysisResult>(cleanJson, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            return rca ?? throw new InvalidOperationException("Deserialization failed.");
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "JSON Error. Raw content was: {Raw}", rawContent);
            return RootCauseAnalysisResult.CreateFailure(ex.Message);
        }
    }

    // Helper to strip Markdown and extra text
    private static string CleanJsonString(string input)
    {
        var start = input.IndexOf('{');
        var end = input.LastIndexOf('}');

        if (start == -1 || end == -1 || end <= start)
            throw new InvalidOperationException("No valid JSON found in AI response.");

        return input.Substring(start, end - start + 1);
    }

    // Helper to avoid repetitive code
    private static string GetStringOrDefault(JsonElement element, string prop, string @default) =>
        element.TryGetProperty(prop, out var val) ? val.GetString() ?? @default : @default;
}
