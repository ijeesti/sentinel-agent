using SentinelAgent.Domain.Aggregates;
using SentinelAgent.Domain.Interfaces;
using SentinelAgent.Domain.ValueObjects;
using System.Collections.Concurrent;

namespace SentinelAgent.Infrastructure.Repositories;

public sealed class InMemoryIncidentRepository : IIncidentRepository
{
    private readonly ConcurrentDictionary<Guid, IncidentTicket> _store = new();

    public Task<IncidentTicket?> FindByIdAsync(
        TicketId id,
        CancellationToken cancellationToken = default)
    {
        _store.TryGetValue(id.Value, out var ticket);
        return Task.FromResult(ticket);
    }

    public Task<IReadOnlyList<IncidentTicket>> FindAllAsync(
        CancellationToken cancellationToken = default)
    {
        IReadOnlyList<IncidentTicket> tickets = _store.Values.ToList();
        return Task.FromResult(tickets);
    }

    public Task SaveAsync(IncidentTicket ticket, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(ticket);
        _store[ticket.Id.Value] = ticket;
        ticket.ClearDomainEvents();
        return Task.CompletedTask;
    }
}

