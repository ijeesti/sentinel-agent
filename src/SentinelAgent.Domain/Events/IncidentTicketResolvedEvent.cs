using SentinelAgent.Domain.Interfaces;
using SentinelAgent.Domain.ValueObjects;

namespace SentinelAgent.Domain.Events;

public sealed record IncidentTicketResolvedEvent(
    TicketId TicketId,
    DateTimeOffset OccurredAt) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
}
