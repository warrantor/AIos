namespace AIos.Core.Application.Llm;

/// <summary>Single chunk from a streaming LLM completion.</summary>
public sealed record LlmStreamChunk(
    string? Token = null,
    string? FinishReason = null,
    int? InputTokenCount = null,
    int? OutputTokenCount = null,
    string? ModelUsed = null,
    string? ProviderMetadata = null);
