using SentinelAgent.Domain.Aggregates;
using SentinelAgent.Domain.ValueObjects;

namespace SentinelAgent.Domain.Interfaces;

public interface IIncidentRepository
{
    Task<IncidentTicket?> FindByIdAsync(TicketId id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<IncidentTicket>> FindAllAsync(CancellationToken cancellationToken = default);
    Task SaveAsync(IncidentTicket ticket, CancellationToken cancellationToken = default);
}
