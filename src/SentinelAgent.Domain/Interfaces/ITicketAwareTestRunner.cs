using SentinelAgent.Domain.Domains;

namespace SentinelAgent.Domain.Interfaces;

/// <summary>
/// Runs a test informed by its linked ticket's acceptance criteria.
/// </summary>
public interface ITicketAwareTestRunner
{
    Task<TestRunResult> RunAsync(
        TicketAwareTestRequest request,
        CancellationToken cancellationToken = default);
}
