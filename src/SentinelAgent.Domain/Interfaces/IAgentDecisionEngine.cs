using SentinelAgent.Domain.Domains;

namespace SentinelAgent.Domain.Interfaces;

public interface IAgentDecisionEngine
{
    AgentDecision Decide(double confidenceScore, int occurrenceCount);
}