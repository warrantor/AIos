using System.Runtime.CompilerServices;
using AIos.Core.Application.Llm;
using Anthropic.SDK;
using Anthropic.SDK.Messaging;

namespace AIos.Core.Infrastructure.Anthropic;

/// <summary>Anthropic Claude-backed implementation of <see cref="ILlmProvider" /> using Anthropic.SDK 5.x Messages API.</summary>
public sealed class AnthropicProvider : ILlmProvider {
    public const string Key = "Anthropic";
    private readonly AnthropicClient _client;
    private readonly int _maxTokens;
    private readonly string _model;

    public AnthropicProvider(AnthropicClient client, string model = "claude-3-5-sonnet-20241022", int maxTokens = 1024) {
        _client = client;
        _model = model;
        _maxTokens = maxTokens;
    }

    /// <inheritdoc />
    public async Task<LlmCompletionResult> CompleteAsync(LlmCompletionRequest request, CancellationToken ct = default) {
        var messages = ToMessages(request.Messages);
        var parameters = new MessageParameters {
            Messages = messages,
            MaxTokens = _maxTokens,
            Model = _model,
            Stream = false
        };
        var response = await _client.Messages.GetClaudeMessageAsync(parameters, ct);
        var text = response?.Message?.ToString();
        return new LlmCompletionResult(text);
    }

    /// <inheritdoc />
    public async IAsyncEnumerable<LlmStreamChunk> StreamCompleteAsync(
        LlmCompletionRequest request,
        [EnumeratorCancellation] CancellationToken ct = default) {
        var messages = ToMessages(request.Messages);
        var parameters = new MessageParameters {
            Messages = messages,
            MaxTokens = _maxTokens,
            Model = _model,
            Stream = true
        };

        await foreach (var res in _client.Messages.StreamClaudeMessageAsync(parameters, ct)) {
            if (res?.Delta?.Text is { Length: > 0 } text) {
                yield return new LlmStreamChunk(text);
            }
        }
    }

    /// <summary>Converts <see cref="LlmMessage"/> list to Anthropic SDK <see cref="Message"/> list (Messages API).</summary>
    private static List<Message> ToMessages(IReadOnlyList<LlmMessage> messages) {
        var list = new List<Message>(messages.Count);
        foreach (var m in messages) {
            var role = m.Role.Trim().ToLowerInvariant();
            var roleType = role switch {
                "assistant" => RoleType.Assistant,
                "user" or "system" or _ => RoleType.User
            };
            list.Add(new Message(roleType, m.Content));
        }
        return list;
    }
}