namespace SentinelAgent.Domain.Domains;

public record RootCauseAnalysisResult(
    string Summary,
    string TechnicalDetail,
    double ConfidenceScore,
    string? SuggestedFix,
    IReadOnlyList<string> PossibleCauses)
{
    public static RootCauseAnalysisResult CreateFailure(string reason) =>
        new(
            Summary: "Error during analysis",
            TechnicalDetail: reason,
            ConfidenceScore: 0,
            SuggestedFix: "Retry the operation or check the logs.",
            PossibleCauses: new List<string>().AsReadOnly()
        );
}