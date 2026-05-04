using SentinelAgent.Domain.Enums;
using SentinelAgent.Domain.Events;
using SentinelAgent.Domain.Interfaces;
using SentinelAgent.Domain.ValueObjects;

namespace SentinelAgent.Domain.Aggregates;

/// <summary>
/// Aggregate root representing an AI-generated QA bug ticket.
/// Encapsulates all domain logic around ticket lifecycle and validation.
/// </summary>
public sealed class IncidentTicket
{
    private readonly List<IDomainEvent> _domainEvents = [];

    public TicketId Id { get; private init; }
    public TicketTitle Title { get; private set; }
    public TicketDescription Description { get; private set; }
    public RootCauseAnalysis RootCause { get; private set; }
    public AcceptanceCriteria AcceptanceCriteria { get; private set; }
    public ReproductionSteps ReproductionSteps { get; private set; }
    public TicketSeverity Severity { get; private set; }
    public TicketStatus Status { get; private set; }
    public FailureContext FailureContext { get; private init; }
    public DateTimeOffset CreatedAt { get; private init; }
    public DateTimeOffset? UpdatedAt { get; private set; }

    public IReadOnlyList<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    private IncidentTicket() { } // EF / deserialization

    private IncidentTicket(
        TicketId id,
        TicketTitle title,
        TicketDescription description,
        RootCauseAnalysis rootCause,
        AcceptanceCriteria acceptanceCriteria,
        ReproductionSteps reproductionSteps,
        TicketSeverity severity,
        FailureContext failureContext,
        TicketStatus  status= TicketStatus.Open)
    {
        Id = id;
        Title = title;
        Description = description;
        RootCause = rootCause;
        AcceptanceCriteria = acceptanceCriteria;
        ReproductionSteps = reproductionSteps;
        Severity = severity;
        FailureContext = failureContext;
        Status = status;
        CreatedAt = DateTimeOffset.UtcNow;

        _domainEvents.Add(new IncidentTicketCreatedEvent(Id, Title, Severity, CreatedAt));
    }

    public static IncidentTicket Create(
        TicketTitle title,
        TicketDescription description,
        RootCauseAnalysis rootCause,
        AcceptanceCriteria acceptanceCriteria,
        ReproductionSteps reproductionSteps,
        TicketSeverity severity,
        FailureContext failureContext) =>
        new(
            TicketId.New(),
            title,
            description,
            rootCause,
            acceptanceCriteria,
            reproductionSteps,
            severity,
            failureContext,
            TicketStatus.Open);
    
    public void MarkAsResolved()
    {
        if (Status == TicketStatus.Resolved)
        {
            throw new InvalidOperationException($"Ticket {Id} is already resolved.");
        }

        Status = TicketStatus.Resolved;
        UpdatedAt = DateTimeOffset.UtcNow;
        _domainEvents.Add(new IncidentTicketResolvedEvent(Id, UpdatedAt.Value));
    }

    public void UpdateRootCause(RootCauseAnalysis updatedRootCause)
    {
        RootCause = updatedRootCause ?? throw new ArgumentNullException(nameof(updatedRootCause));
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void ClearDomainEvents() => _domainEvents.Clear();
}


