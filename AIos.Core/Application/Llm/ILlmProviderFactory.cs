namespace AIos.Core.Application.Llm;

/// <summary>Resolves <see cref="ILlmProvider"/> by provider id (e.g. "OpenAI", "Anthropic").</summary>
public interface ILlmProviderFactory
{
    /// <summary>Gets the provider for the given id, or the default provider when <paramref name="providerId"/> is null or empty.</summary>
    ILlmProvider GetProvider(string? providerId = null);
}
