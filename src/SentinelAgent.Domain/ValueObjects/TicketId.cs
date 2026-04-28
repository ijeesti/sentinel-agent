namespace SentinelAgent.Domain.ValueObjects;

public sealed record TicketId(Guid Value)
{
    public static TicketId New() => new(Guid.NewGuid());
    public static TicketId From(Guid value) => new(value);
    public override string ToString() => Value.ToString();
}
