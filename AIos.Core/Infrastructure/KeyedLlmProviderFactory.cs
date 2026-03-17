using AIos.Core.Application.Llm;
using AIos.Core.Infrastructure.OpenAi;
using Microsoft.Extensions.DependencyInjection;

namespace AIos.Core.Infrastructure;

/// <summary>Resolves <see cref="ILlmProvider" /> from keyed services using the given default key.</summary>
public sealed class KeyedLlmProviderFactory : ILlmProviderFactory {
    private readonly string _defaultKey;
    private readonly IKeyedServiceProvider _keyed;

    public KeyedLlmProviderFactory(IKeyedServiceProvider keyed, string defaultKey = OpenAiProvider.Key) {
        _keyed = keyed;
        _defaultKey = defaultKey;
    }

    /// <inheritdoc />
    public ILlmProvider GetProvider(string? providerId = null) {
        var key = string.IsNullOrWhiteSpace(providerId) ? _defaultKey : providerId;
        var provider = _keyed.GetKeyedService<ILlmProvider>(key);

        return provider ?? _keyed.GetRequiredKeyedService<ILlmProvider>(_defaultKey);
    }
}