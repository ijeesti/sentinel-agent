namespace SentinelAgent.Application.Agents;

public class DeduplicationStore
{
    private readonly Dictionary<string, int> _counts = [];

    public bool TryRegister(string fingerprint, out int count)
    {
        if (_counts.TryGetValue(fingerprint, out int value))
        {
            _counts[fingerprint] = ++value;
            count = value;
            return false; // duplicate
        }

        _counts[fingerprint] = 1;
        count = 1;
        return true; // new
    }
}