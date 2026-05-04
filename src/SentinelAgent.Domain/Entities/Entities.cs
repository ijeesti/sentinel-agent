namespace SentinelAgent.Domain.Entities;

public class FailureRecord
{
    public string Fingerprint { get; private set; } = default!;

    public string? Title { get; private set; }

    public int OccurrenceCount { get; private set; }

    public DateTime FirstSeenUtc { get; private set; }

    public DateTime LastSeenUtc { get; private set; }

    private FailureRecord() { } // EF

    public FailureRecord(string fingerprint, string? title)
    {
        Fingerprint = fingerprint;
        Title = title;
        OccurrenceCount = 1;
        FirstSeenUtc = DateTime.UtcNow;
        LastSeenUtc = DateTime.UtcNow;
    }

    public void Increment()
    {
        OccurrenceCount++;
        LastSeenUtc = DateTime.UtcNow;
    }
}