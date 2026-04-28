using SentinelAgent.Domain.Interfaces;

namespace SentinelAgent.Host;

internal record AgentBundle(
    ITicketGeneratorAgent Generator,
    IRootCauseAnalyzerAgent Analyzer,
    ITicketAwareTestRunner Runner,
    IIncidentRepository Repo);
