namespace SentinelAgent.Domain.ValueObjects;

public sealed record RootCauseAnalysis
{
    public string Summary { get; }
    public string TechnicalDetail { get; }
    public double ConfidenceScore { get; }
    public string? SuggestedFix { get; }

    public RootCauseAnalysis(
        string summary,
        string technicalDetail,
        double confidenceScore,
        string? suggestedFix = null)
    {
        //if (string.IsNullOrWhiteSpace(summary))
        //    throw new ArgumentException("Root cause summary cannot be empty.", nameof(summary));
        //if (confidenceScore is < 0 or > 1)
        //    throw new ArgumentOutOfRangeException(nameof(confidenceScore), "Confidence score must be between 0 and 1.");

        Summary = summary.Trim();
        TechnicalDetail = technicalDetail?.Trim() ?? "Please check the summary";
        ConfidenceScore = confidenceScore;
        SuggestedFix = suggestedFix?.Trim();
    }
}