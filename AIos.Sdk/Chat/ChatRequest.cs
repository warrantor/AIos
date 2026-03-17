namespace AIos.Sdk.Chat;

public sealed record ChatRequest(IReadOnlyList<ChatMessage> History, string? ProviderId = null);