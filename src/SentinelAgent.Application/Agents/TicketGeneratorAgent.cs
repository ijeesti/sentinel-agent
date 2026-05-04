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
public class TicketGeneratorAgent(
    Kernel kernel,
    ILogger<TicketGeneratorAgent> logger,
    DeduplicationStore dedupStore,
    IAgentDecisionEngine decisionEngine,
    IReportGenerator reportGenerator) : ITicketGeneratorAgent
{
    private readonly ILogger<TicketGeneratorAgent> logger = logger;
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        NumberHandling = JsonNumberHandling.AllowReadingFromString,
        AllowTrailingCommas = true,
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Skip
    };

    public async Task<IncidentTicket?> GenerateTicketAsync(
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
            new() { ["rawStackTrace"] = request.RawFailureInput },
            cancellationToken);

        var arguments = new KernelArguments
        {
            ["inputType"] = request.InputType.ToString(),
            ["rawInput"] = sanitizedResult.GetValue<string>(),
            ["additionalContext"] = request.AdditionalContext ?? ""
        };

        var result = await kernel.InvokePromptAsync(
            AgentPrompts.TicketGenerator,
            arguments,
            cancellationToken: cancellationToken);

        var rawJson = result.GetValue<string>() ?? throw new Exception("No response");
        var cleanJson = CleanMarkdown(rawJson);
        IncidentTicketDto dto;

        try
        {
            dto = JsonSerializer.Deserialize<IncidentTicketDto>(cleanJson, _jsonOptions)!;
        }
        catch (JsonException ex)
        {
            logger.LogError("JSON Error at {Path}: {Message}", ex.Path, ex.Message);
            throw;
        }

        var fingerprint = FailureFingerprint.Generate(request.RawFailureInput);
        var isNew = dedupStore.TryRegister(fingerprint, out var count);
        var decision = decisionEngine.Decide(dto.RootCause.ConfidenceScore, count);

        if (decision.Action == AgentAction.Ignore || !isNew)
        {
            logger.LogInformation("Skipping ticket creation. Action {0}, {IsNew}"
                , decision.Action, isNew);
            return null;
        }

        logger.LogInformation(
         "Decision: {Action} | Confidence: {Confidence} | Count: {Count}",
         decision.Action,
         dto.RootCause.ConfidenceScore,
         count);

        var severity = decision.Action == AgentAction.Escalate
            ? TicketSeverity.Critical
            : Enum.Parse<TicketSeverity>(dto.Severity);

        //Create Ticket
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
            severity: severity,
            failureContext: new FailureContext(
                request.RawFailureInput,
                FailureInputType.StackTrace,
                request.SourceLocation)
        );

        //Report
        var html = reportGenerator.GenerateIncidentReport(ticket);
        const string reportFolder = "Reports";
        Directory.CreateDirectory(reportFolder);

        string fileName = Path.Combine(
            reportFolder,
            $"TicketReport_{DateTime.Now:yyyyMMdd_HHmmss}_{Guid.NewGuid():N}.html");

        await File.WriteAllTextAsync(fileName, html, cancellationToken);
        return ticket;
    }

    public static string CleanMarkdown(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return string.Empty;
        }

        int start = input.IndexOf('{');
        int end = input.LastIndexOf('}');

        return (start == -1 || end == -1)
            ? input
            : input[start..(end + 1)];
    }
}