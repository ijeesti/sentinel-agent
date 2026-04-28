using SentinelAgent.Domain.Enums;

namespace SentinelAgent.Domain.ValueObjects;

/// <summary>
/// Captures the raw failure input that triggered the AI analysis.
/// </summary>
public sealed record FailureContext
{
    public string RawInput { get; }
    public FailureInputType InputType { get; }
    public string? SourceLocation { get; }

    public FailureContext(string rawInput, FailureInputType inputType, string? sourceLocation = null)
    {
        if (string.IsNullOrWhiteSpace(rawInput))
            throw new ArgumentException("Raw input cannot be empty.", nameof(rawInput));

        RawInput = rawInput.Trim();
        InputType = inputType;
        SourceLocation = sourceLocation?.Trim();
    }
}


