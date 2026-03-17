using System.Runtime.CompilerServices;
using AIos.Core.Application.Llm;
using Microsoft.Extensions.AI;

namespace AIos.Core.Infrastructure.OpenAi;

/// <summary>OpenAI-backed implementation of <see cref="ILlmProvider" /> using Microsoft.Extensions.AI <see cref="IChatClient" />.</summary>
public sealed class OpenAiProvider : ILlmProvider {
    public const string Key = "OpenAI";
    private readonly IChatClient _client;

    public OpenAiProvider(IChatClient client) {
        _client = client;
    }

    /// <inheritdoc />
    public async Task<LlmCompletionResult> CompleteAsync(LlmCompletionRequest request, CancellationToken ct = default) {
        var messages = ToChatMessages(request.Messages);
        var fullText = new List<string>();

        await foreach (var update in _client.GetStreamingResponseAsync(messages, cancellationToken: ct)) {
            if (update.Text is { Length: > 0 } text) {
                fullText.Add(text);
            }
        }
        return new LlmCompletionResult(string.Concat(fullText));
    }

    /// <inheritdoc />
    public async IAsyncEnumerable<LlmStreamChunk> StreamCompleteAsync(
        LlmCompletionRequest request,
        [EnumeratorCancellation] CancellationToken ct = default) {
        var messages = ToChatMessages(request.Messages);

        await foreach (var update in _client.GetStreamingResponseAsync(messages, cancellationToken: ct)) {
            if (update.Text is { Length: > 0 } text) {
                yield return new LlmStreamChunk(text);
            }
        }
    }

    private static IReadOnlyList<ChatMessage> ToChatMessages(IReadOnlyList<LlmMessage> messages) {
        return messages
            .Select(m => new ChatMessage(new ChatRole(m.Role), m.Content))
            .ToList();
    }
}