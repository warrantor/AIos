namespace AIos.Core.Application.Llm;

/// <summary>Provider-agnostic request for LLM completion (chat).</summary>
public sealed record LlmCompletionRequest(IReadOnlyList<LlmMessage> Messages);
