namespace AIos.Core.Application.Llm;

/// <summary>Single message in an LLM completion request (role + content).</summary>
public sealed record LlmMessage(string Role, string Content);
