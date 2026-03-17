namespace AIos.Core.Application.Llm;

/// <summary>Provider-agnostic interface for LLM completion (chat).</summary>
public interface ILlmProvider {
    /// <summary>Run a non-streaming completion.</summary>
    Task<LlmCompletionResult> CompleteAsync(LlmCompletionRequest request, CancellationToken ct = default);

    /// <summary>Run a streaming completion; yields chunks with token text and optional finish/metadata.</summary>
    IAsyncEnumerable<LlmStreamChunk> StreamCompleteAsync(LlmCompletionRequest request, CancellationToken ct = default);
}