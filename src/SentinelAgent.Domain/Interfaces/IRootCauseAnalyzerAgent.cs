using SentinelAgent.Domain.Domains;

namespace SentinelAgent.Domain.Interfaces;

/// <summary>
/// Analyzes a failure to determine its root cause with a confidence score.
/// </summary>
public interface IRootCauseAnalyzerAgent
{
    Task<RootCauseAnalysisResult> AnalyzeAsync(
        AnalyzeFailureRequest request,
        CancellationToken cancellationToken = default);
}