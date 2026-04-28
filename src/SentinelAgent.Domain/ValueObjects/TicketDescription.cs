
namespace SentinelAgent.Domain.ValueObjects;

public sealed record TicketDescription
{
    public string Value { get; }

    public TicketDescription(string value)
    {
        //if (string.IsNullOrWhiteSpace(value))
        //    throw new ArgumentException("Ticket description cannot be empty.", nameof(value));

        Value = value?.Trim() ?? "Ticket description is empty";
    }

    public override string ToString() => Value;
}
