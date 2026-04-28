using Microsoft.Extensions.Logging;

namespace SentinelAgent.Host;

// Performance: Source-Generated Logging
// This prevents string allocations when the log level is disabled.
internal static partial class LogActions
{
    [LoggerMessage(Level = LogLevel.Information, Message = "Calling {AgentName}...")]
    public static partial void LogCallingAgent(ILogger logger, string agentName);
}