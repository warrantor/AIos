using System.Runtime.CompilerServices;
using Microsoft.Extensions.AI;

namespace AIos.Core.Application.Chat;

public interface IChatOrchestrator {
    IAsyncEnumerable<string> StreamAsync(IReadOnlyList<ChatMessage> history, CancellationToken ct = default);
}

public sealed class ChatOrchestrator : IChatOrchestrator {
    private readonly IChatClient _client;

    public ChatOrchestrator(IChatClient client) {
        _client = client;
    }

    public async IAsyncEnumerable<string> StreamAsync(IReadOnlyList<ChatMessage> history, [EnumeratorCancellation] CancellationToken ct = default) {
        await foreach (var update in _client.GetStreamingResponseAsync(history, cancellationToken: ct)) {
            if (update.Text is { Length: > 0 } text) {
                yield return text;
            }
        }
    }
}