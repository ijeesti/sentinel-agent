namespace SentinelAgent.Infrastructure.Configuration;

/// <summary>
/// 
/// </summary>
public sealed class OllamaOptions
{
    public const string SectionName = "Ollama";
    public string BaseUrl { get; set; } = "http://localhost:11434";
    public string ModelId { get; set; } = "llama3.1";
    public int TimeoutSeconds { get; set; } = 300;
    public int MaxTokens { get; set; } = 2048;
}

