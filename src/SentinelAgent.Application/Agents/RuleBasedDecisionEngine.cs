using SentinelAgent.Domain.Domains;
using SentinelAgent.Domain.Enums;
using SentinelAgent.Domain.Interfaces;

namespace SentinelAgent.Application.Agents;

public class RuleBasedDecisionEngine : IAgentDecisionEngine
{
    public AgentDecision Decide(double confidenceScore, int occurrenceCount)
    {
        if (confidenceScore < 0.5)
        {
            return new AgentDecision(
                AgentAction.Ignore,
                "Low confidence score from AI");
        }

        if (occurrenceCount > 5)
        {
            return new AgentDecision(
                AgentAction.Escalate,
                "High frequency failure detected");
        }

        return new AgentDecision(
            AgentAction.CreateTicket,
            "Valid issue identified");
    }
}