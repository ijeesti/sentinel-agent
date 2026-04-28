namespace SentinelAgent.Domain.ValueObjects;

public sealed record TicketTitle
{
    public string Value { get; }

    public TicketTitle(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Ticket title cannot be empty.", nameof(value));
        if (value.Length > 200)
            throw new ArgumentException("Ticket title cannot exceed 200 characters.", nameof(value));

        Value = value.Trim();
    }
    public override string ToString() => Value;
}
