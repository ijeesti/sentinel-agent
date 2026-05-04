using Microsoft.EntityFrameworkCore;
using SentinelAgent.Domain.Domains;
using SentinelAgent.Domain.Entities;
using SentinelAgent.Domain.Interfaces;
using SentinelAgent.Infrastructure.Persistence;

namespace SentinelAgent.Infrastructure.Repositories;

public class FailureRepository(IncidentDbContext incidentDbContext) : IFailureRepository
{
    public async Task<FailureRegisterResult> RegisterAsync(
        string fingerprint,
        string? title,
        CancellationToken cancellationToken = default)
    {

        // Try to find existing failure
        var existing = await incidentDbContext.Failures.FirstOrDefaultAsync
            (x => x.Fingerprint == fingerprint, cancellationToken);

        if (existing is not null)
        {
            existing.Increment();
            await incidentDbContext.SaveChangesAsync(cancellationToken);
            return new(false, existing.OccurrenceCount);
        }

        // New failure
        var record = new FailureRecord(fingerprint, title);
        incidentDbContext.Failures.Add(record);
        await incidentDbContext.SaveChangesAsync(cancellationToken);
        return new(true, record.OccurrenceCount);
    }
}