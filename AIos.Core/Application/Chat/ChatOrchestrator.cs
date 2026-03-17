using System.Runtime.CompilerServices;
using AIos.Core.Application.Llm;
using SdkMessage = AIos.Sdk.Chat.ChatMessage;

namespace AIos.Core.Application.Chat;

public interface IChatOrchestrator {
    IAsyncEnumerable<string> StreamAsync(IReadOnlyList<SdkMessage> history, string? providerId = null, CancellationToken ct = default);
}

public sealed class ChatOrchestrator : IChatOrchestrator {
    private readonly ILlmProviderFactory _providerFactory;

    public ChatOrchestrator(ILlmProviderFactory providerFactory) {
        _providerFactory = providerFactory;
    }

    public async IAsyncEnumerable<string> StreamAsync(IReadOnlyList<SdkMessage> history, string? providerId = null,
        [EnumeratorCancellation] CancellationToken ct = default) {
        var request = new LlmCompletionRequest(history.Select(m => new LlmMessage(m.Role.Value, m.Content)).ToList());
        var provider = _providerFactory.GetProvider(providerId);

        await foreach (var chunk in provider.StreamCompleteAsync(request, ct)) {
            if (chunk.Token is { Length: > 0 } text) {
                yield return text;
            }
        }
    }
}