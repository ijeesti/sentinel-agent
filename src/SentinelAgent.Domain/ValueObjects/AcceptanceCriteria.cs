namespace SentinelAgent.Domain.ValueObjects;

public sealed record AcceptanceCriteria
{
    public IReadOnlyList<string> Criteria { get; }

    public AcceptanceCriteria(IEnumerable<string> criteria)
    {
        var list = criteria?.Where(c => !string.IsNullOrWhiteSpace(c)).ToList()
                   ?? throw new ArgumentNullException(nameof(criteria));

        if (list.Count == 0)
            throw new ArgumentException("At least one acceptance criterion must be provided.");

        Criteria = list.AsReadOnly();
    }

    public override string ToString() =>
        string.Join(Environment.NewLine, Criteria.Select((c, i) => $"{i + 1}. {c}"));
}

