namespace AIos.Core.Application.Llm;

/// <summary>Result of a non-streaming LLM completion.</summary>
public sealed record LlmCompletionResult(
    string? FullText,
    string? FinishReason = null,
    int? InputTokenCount = null,
    int? OutputTokenCount = null,
    string? ModelUsed = null,
    string? ProviderMetadata = null);
