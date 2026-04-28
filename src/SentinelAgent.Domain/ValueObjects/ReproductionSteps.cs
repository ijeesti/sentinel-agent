namespace SentinelAgent.Domain.ValueObjects;

public sealed record ReproductionSteps
{
    public IReadOnlyList<string> Steps { get; }

    public ReproductionSteps(IEnumerable<string> steps)
    {
        var list = steps?
            .Where(s => !string.IsNullOrWhiteSpace(s))
            .ToList() ?? throw new ArgumentNullException(nameof(steps));

        Steps = list.AsReadOnly();
    }

    public override string ToString() =>
        string.Join(Environment.NewLine, Steps.Select((s, i) => $"Step {i + 1}: {s}"));
}

