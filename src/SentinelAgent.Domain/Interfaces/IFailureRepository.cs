
using SentinelAgent.Domain.Domains;

namespace SentinelAgent.Domain.Interfaces;

public interface IFailureRepository
{
    Task<FailureRegisterResult> RegisterAsync(
        string fingerprint,
        string? title,
        CancellationToken cancellationToken = default);
}