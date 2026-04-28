using SentinelAgent.Domain.Aggregates;
using SentinelAgent.Domain.Enums;
using SentinelAgent.Domain.Interfaces;
using SentinelAgent.Domain.ValueObjects;

namespace SentinelAgent.Domain.Events;

public sealed record IncidentTicketCreatedEvent(
    TicketId TicketId,
    TicketTitle Title,
    TicketSeverity Severity,
    DateTimeOffset OccurredAt) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
}


