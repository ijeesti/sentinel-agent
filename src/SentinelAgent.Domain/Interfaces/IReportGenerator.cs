using SentinelAgent.Domain.Aggregates;

namespace SentinelAgent.Domain.Interfaces;

public interface IReportGenerator
{
    string GenerateIncidentReport(IncidentTicket ticket);
}
