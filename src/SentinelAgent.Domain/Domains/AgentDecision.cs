using SentinelAgent.Domain.Enums;

namespace SentinelAgent.Domain.Domains;

public record AgentDecision(
    AgentAction Action,
    string Reason
);
