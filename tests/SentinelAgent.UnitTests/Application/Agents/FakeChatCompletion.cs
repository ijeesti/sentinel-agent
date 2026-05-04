using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using System.Runtime.CompilerServices;

namespace SentinelAgent.UnitTests.Application.Agents;

// <summary>
/// In-process stub for IChatCompletionService.
/// Returns a fixed string so tests never hit a real LLM.
/// </summary>
public sealed class FakeChatCompletion(string? response) : IChatCompletionService
{
    public bool SimulateCancellation { get; set; }

    public IReadOnlyDictionary<string, object?> Attributes => new Dictionary<string, object?>();

    public Task<IReadOnlyList<ChatMessageContent>> GetChatMessageContentsAsync(
        ChatHistory chatHistory,
        PromptExecutionSettings? executionSettings = null,
        Kernel? kernel = null,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (SimulateCancellation)
            throw new OperationCanceledException(cancellationToken);

        if (response is null)
            throw new InvalidOperationException("LLM returned null response for test evaluation.");

        IReadOnlyList<ChatMessageContent> list =
        [
            new ChatMessageContent(AuthorRole.Assistant, response)
        ];
        return Task.FromResult(list);
    }

    public async IAsyncEnumerable<StreamingChatMessageContent> GetStreamingChatMessageContentsAsync(
        ChatHistory chatHistory,
        PromptExecutionSettings? executionSettings = null,
        Kernel? kernel = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        await Task.CompletedTask;
        if (response is not null)
            yield return new StreamingChatMessageContent(AuthorRole.Assistant, response);
    }
}