using SentinelAgent.Domain.Enums;

namespace SentinelAgent.Domain.Domains;
// ──────────────────────────────────────────────────────
// Request / Response DTOs/ TODO: Separate class..
// ──────────────────────────────────────────────────────

public sealed record GenerateTicketRequest(
    string RawFailureInput,
    FailureInputType InputType,
    string? SourceLocation = null,
    string? AdditionalContext = null);

public sealed record AnalyzeFailureRequest(
    string RawFailureInput,
    FailureInputType InputType,
    string? CodeSnippet = null);

public sealed record TicketAwareTestRequest(
    string TicketId,
    string TicketDescription,
    IReadOnlyList<string> AcceptanceCriteria,
    string TestCode,
    string? TestOutput = null);

public sealed record TestRunResult(
    bool Passed,
    string Summary,
    IReadOnlyList<string> CriteriaResults,
    string? FailureReason,
    string? SuggestedFix);

public record FailureScenario(
    string RawInput,
    FailureInputType Type,
    string Context);


public record IncidentTicketDto(
    string Title,
    string Description,
    RootCauseAnalysisDto RootCause,
    List<string> AcceptanceCriteria,
    List<string> ReproductionSteps,
    string Severity
);

public record RootCauseAnalysisDto(
    string Summary,
    string TechnicalDetail,
    double ConfidenceScore,
    string? SuggestedFix
);

public record FailureRegisterResult(
    bool IsNew,
    int Count);