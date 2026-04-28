using SentinelAgent.Application.Plugins;
using SentinelAgent.Application.Prompts;
using SentinelAgent.Domain.Aggregates;
using SentinelAgent.Domain.Domains;
using SentinelAgent.Domain.Enums;
using SentinelAgent.Domain.Interfaces;
using SentinelAgent.Domain.ValueObjects;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using System.Text;
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

    public async Task<IncidentTicket> GenerateTicketAsyncOption2(
        GenerateTicketRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        logger.LogInformation(
            "Generating Production\\QA Incident ticket for {InputType} input ({Length} chars)",
            request.InputType, request.RawFailureInput.Length);

        var arguments = new KernelArguments
        {
            ["rawInput"] = request.RawFailureInput,
            ["inputType"] = request.InputType.ToString(),
            ["additionalContext"] = request.AdditionalContext is not null
                ? $"## Additional Context\n{request.AdditionalContext}"
                : string.Empty
        };

        var ticket = await kernel.InvokePromptAsync<IncidentTicket>(
            AgentPrompts.TicketGenerator,
            arguments,
            cancellationToken: cancellationToken);

        logger.LogDebug("Raw LLM response for ticket generation: {Response}", ticket);
        //var ticket = ParseTicketFromJson(rawJson, request);
        var htmlBuilder = new StringBuilder();
        htmlBuilder.Append("<html><head><style>...</style></head><body>");
        htmlBuilder.Append($@"
            <div class='card'>
                <h2>Ticket Id: {ticket.Id}</h2>
                <p><strong>Title:</strong> {ticket.Title}</p>
                <div class='status-badge'>{ticket.Status}</div>
                <h3>Root Cause</h3>
                <p>{ticket.RootCause.Summary}</p>
            </div>");
        const string reportFolder = "Reports";
        Directory.CreateDirectory(reportFolder);
        string fileName = Path.Combine(reportFolder, $"TicketReport_{DateTime.Now:yyyyMMdd_HHmmss}.html");
        File.WriteAllText(fileName, htmlBuilder.ToString());
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

    private static IncidentTicket? ParseTicketFromJson(string rawJson, GenerateTicketRequest request)
    {
        // Strip markdown code fences if the LLM ignores our instructions
        var json = rawJson
            .Replace("```json", string.Empty, StringComparison.OrdinalIgnoreCase)
            .Replace("```", string.Empty)
            .Trim();

        try
        {
            // Local LLMs sometimes truncate mid-response — close any open braces/brackets
            json = RepairTruncatedJson(json);

            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            var title = new TicketTitle(root.GetProperty("title").GetString()!);
            var description = new TicketDescription(root.GetProperty("description").GetString()!);

            var rcNode = root.GetProperty("rootCause");
            var rootCause = new RootCauseAnalysis(
                summary: rcNode.GetProperty("summary").GetString()!,
                technicalDetail: rcNode.GetProperty("technicalDetail").GetString()!,
                confidenceScore: rcNode.GetProperty("confidenceScore").GetDouble(),
                suggestedFix: rcNode.TryGetProperty("suggestedFix", out var sf) ? sf.GetString() : null);

            // Use TryGetProperty for arrays — they may be missing if response was truncated
            var acItems = root.TryGetProperty("acceptanceCriteria", out var acNode)
                ? acNode.EnumerateArray().Select(e => e.GetString()!).Where(s => s != null).ToList()
                : ["Acceptance criteria not generated — retry or increase MaxTokens."];
            var acceptanceCriteria = new AcceptanceCriteria(acItems);

            var steps = root.TryGetProperty("reproductionSteps", out var stepsNode)
                ? stepsNode.EnumerateArray().Select(e => e.GetString()!).Where(s => s != null).ToList()
                : ["Reproduction steps not generated — retry or increase MaxTokens."];
            var reproductionSteps = new ReproductionSteps(steps);

            var severityRaw = root.TryGetProperty("severity", out var sevNode)
                ? sevNode.GetString() ?? "Medium"
                : "Medium";
            var severity = Enum.TryParse<TicketSeverity>(severityRaw, ignoreCase: true, out var parsed)
                ? parsed
                : TicketSeverity.Medium;

            var failureContext = new FailureContext(
                request.RawFailureInput,
                request.InputType,
                request.SourceLocation);

            return IncidentTicket.Create(
                title, description, rootCause,
                acceptanceCriteria, reproductionSteps,
                severity, failureContext);
        }
        catch (JsonException ex)
        {
            Console.WriteLine(ex.ToString());
            return null;
        }
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