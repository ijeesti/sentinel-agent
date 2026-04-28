using SentinelAgent.Application.Plugins;
using SentinelAgent.Application.Prompts;
using SentinelAgent.Domain.Aggregates;
using SentinelAgent.Domain.Domains;
using SentinelAgent.Domain.Enums;
using SentinelAgent.Domain.Interfaces;
using SentinelAgent.Domain.ValueObjects;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SentinelAgent.Application.Agents;

/// <summary>
/// Uses Semantic Kernel + Ollama to transform a raw failure input
/// into a fully structured QA ticket with root cause and acceptance criteria.
/// </summary>
public sealed class TicketGeneratorAgent(
    Kernel kernel,
    IReportGenerator reportGenerator,
    ILogger<TicketGeneratorAgent> logger) : ITicketGeneratorAgent
{

    public async Task<IncidentTicket> GenerateTicketAsync(
        GenerateTicketRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!kernel.Plugins.TryGetPlugin(nameof(SecurityGuardrailPlugin), out _))
        {
            kernel.Plugins.AddFromType<SecurityGuardrailPlugin>(nameof(SecurityGuardrailPlugin));
        }

        var sanitizedResult = await kernel.InvokeAsync(
            nameof(SecurityGuardrailPlugin),
            nameof(SecurityGuardrailPlugin.Sanitize),
            new() { ["rawStackTrace"] = request.RawFailureInput }, cancellationToken);

        var arguments = new KernelArguments
        {
            ["inputType"] = request.InputType.ToString(),
            ["rawInput"] = sanitizedResult.GetValue<string>(),
            ["additionalContext"] = request.AdditionalContext ?? ""
        };

        var result = await kernel.InvokePromptAsync(
            AgentPrompts.TicketGenerator,
            arguments);
        var rawJson = result.GetValue<string>() ?? throw new Exception("No response");
        var cleanJson = CleanMarkdown(rawJson);

        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            NumberHandling = JsonNumberHandling.AllowReadingFromString,
            AllowTrailingCommas = true,
            UnmappedMemberHandling = JsonUnmappedMemberHandling.Skip
        };
        IncidentTicketDto? dto;
        try
        {
            dto = JsonSerializer.Deserialize<IncidentTicketDto>(cleanJson, options);
        }
        catch (JsonException ex)
        {
            logger.LogError("JSON Error at {Path}: {Message}", ex.Path, ex.Message);
            Console.WriteLine("--- RAW JSON FROM AI ---");
            Console.WriteLine(cleanJson);
            Console.WriteLine("------------------------");
            throw;
        }

        var ticket = IncidentTicket.Create(
            title: new TicketTitle(dto.Title),
            description: new TicketDescription(dto.Description),
            rootCause: new RootCauseAnalysis(
                dto.RootCause.Summary,
                dto.RootCause.TechnicalDetail,
                dto.RootCause.ConfidenceScore,
                dto.RootCause.SuggestedFix),
            acceptanceCriteria: new AcceptanceCriteria(dto.AcceptanceCriteria),
            reproductionSteps: new ReproductionSteps(dto.ReproductionSteps),
            severity: Enum.Parse<TicketSeverity>(dto.Severity),
            failureContext: new FailureContext(
                request.RawFailureInput,
                FailureInputType.StackTrace,
                request.SourceLocation)
        );
         var html = reportGenerator.GenerateIncidentReport(ticket);
        const string reportFolder = "Reports";
        Directory.CreateDirectory(reportFolder);
        string fileName = Path.Combine(reportFolder, $"TicketReport_{DateTime.Now:yyyyMMdd_HHmmss}.html");
        File.WriteAllText(fileName, html);
        return ticket;
    }

    public static string CleanMarkdown(string input)
    {
        if (string.IsNullOrWhiteSpace(input)) return string.Empty;

        // Find the actual start and end of the JSON object
        int start = input.IndexOf('{');
        int end = input.LastIndexOf('}');

        // If we can't find braces, the AI probably didn't return JSON at all
        if (start == -1 || end == -1) return input;

        // C# Range operator (..) to extract exactly what's between the braces
        return input[start..(end + 1)];
    }

    /// <summary>
    /// Repairs truncated JSON from local LLMs that stop generating mid-response.
    /// Counts open braces/brackets and closes any that are missing.
    /// </summary>
    private static string RepairTruncatedJson(string json)
    {
        // If it already parses cleanly, nothing to do
        try { JsonDocument.Parse(json); return json; }
        catch (JsonException) { }

        var stack = new Stack<char>();
        bool inString = false;
        bool escaped = false;

        foreach (var ch in json)
        {
            if (escaped) { escaped = false; continue; }
            if (ch == '\\' && inString) { escaped = true; continue; }
            if (ch == '"') { inString = !inString; continue; }
            if (inString) continue;

            if (ch == '{') stack.Push('}');
            else if (ch == '[') stack.Push(']');
            else if ((ch == '}' || ch == ']') && stack.Count > 0 && stack.Peek() == ch)
                stack.Pop();
        }

        var repaired = json.TrimEnd();
        if (inString) repaired += "\"";           // close open string
        while (stack.Count > 0) repaired += stack.Pop(); // close open objects/arrays

        return repaired;
    }
}