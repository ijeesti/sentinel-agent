using SentinelAgent.Domain.Aggregates;
using SentinelAgent.Domain.Domains;

namespace SentinelAgent.Domain.Interfaces;

public interface ITicketGeneratorAgent
{
    Task<IncidentTicket> GenerateTicketAsync(
        GenerateTicketRequest request,
        CancellationToken cancellationToken = default);
}