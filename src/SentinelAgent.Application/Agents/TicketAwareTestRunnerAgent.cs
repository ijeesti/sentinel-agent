using SentinelAgent.Application.Prompts;
using SentinelAgent.Domain.Domains;
using SentinelAgent.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using System.Text.Json;

namespace SentinelAgent.Application.Agents;

public sealed class TicketAwareTestRunnerAgent(Kernel kernel, ILogger<TicketAwareTestRunnerAgent> logger) : ITicketAwareTestRunner
{
    private readonly Kernel _kernel = kernel ?? throw new ArgumentNullException(nameof(kernel));
    private readonly ILogger<TicketAwareTestRunnerAgent> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    public async Task<TestRunResult> RunAsync(
        TicketAwareTestRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        _logger.LogInformation(
            "Evaluating test for ticket {TicketId} against {CriteriaCount} acceptance criteria",
            request.TicketId, request.AcceptanceCriteria.Count);

        // Format acceptance criteria as numbered list for the LLM
        var formattedCriteria = string.Join(
            Environment.NewLine,
            request.AcceptanceCriteria.Select((ac, i) => $"{i + 1}. {ac}"));

        var arguments = new KernelArguments
        {
            ["ticketId"] = request.TicketId,
            ["ticketDescription"] = request.TicketDescription,
            ["acceptanceCriteria"] = formattedCriteria,
            ["testCode"] = request.TestCode,
            ["testOutput"] = request.TestOutput ?? string.Empty
        };

        var response = await _kernel.InvokePromptAsync(
            AgentPrompts.TicketAwareTestRunner,
            arguments,
            cancellationToken: cancellationToken);

        var rawJson = response.GetValue<string>()
            ?? throw new InvalidOperationException("LLM returned null response for test evaluation.");

        _logger.LogDebug("Test runner evaluation: {Response}", rawJson);

        return ParseTestRunResultFromJson(rawJson);
    }

    private static TestRunResult ParseTestRunResultFromJson(string rawJson)
    {
        var json = rawJson
            .Replace("```json", string.Empty, StringComparison.OrdinalIgnoreCase)
            .Replace("```", string.Empty)
            .Trim();

        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        var criteriaResults = root.TryGetProperty("criteriaResults", out var cr)
            ? cr.EnumerateArray().Select(e => e.GetString()!).ToList()
            : new List<string>();

        return new TestRunResult(
            Passed: root.GetProperty("passed").GetBoolean(),
            Summary: root.GetProperty("summary").GetString()!,
            CriteriaResults: criteriaResults.AsReadOnly(),
            FailureReason: root.TryGetProperty("failureReason", out var fr) && fr.ValueKind != JsonValueKind.Null
                ? fr.GetString() : null,
            SuggestedFix: root.TryGetProperty("suggestedFix", out var sf) && sf.ValueKind != JsonValueKind.Null
                ? sf.GetString() : null);
    }
}
