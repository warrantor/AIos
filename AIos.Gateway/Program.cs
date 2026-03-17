using AIos.Core.Application.Chat;
using AIos.Core.Application.Llm;
using AIos.Core.Infrastructure;
using AIos.Core.Infrastructure.Anthropic;
using AIos.Core.Infrastructure.OpenAi;
using Anthropic.SDK;
using FastEndpoints;
using Microsoft.Extensions.AI;
using OpenAI;

var builder = WebApplication.CreateBuilder(args);

var config = builder.Configuration;

// OpenAI: keyed IChatClient and ILlmProvider
builder.Services.AddKeyedSingleton<IChatClient>(OpenAiProvider.Key, (sp, _) => {
    var key = config["Llm:Providers:OpenAI:ApiKey"] ?? config["OPENAI_API_KEY"] ?? "";
    var modelId = config["Llm:Providers:OpenAI:ModelId"] ?? "gpt-4o";
    var openAiClient = new OpenAIClient(key);
    return openAiClient.GetChatClient(modelId).AsIChatClient();
});
builder.Services.AddKeyedSingleton<ILlmProvider, OpenAiProvider>(OpenAiProvider.Key, (sp, _) =>
    new OpenAiProvider(sp.GetRequiredKeyedService<IChatClient>(OpenAiProvider.Key)));

// Anthropic: keyed ILlmProvider
builder.Services.AddKeyedSingleton<ILlmProvider>(AnthropicProvider.Key, (sp, _) => {
    var apiKey = config["Llm:Providers:Anthropic:ApiKey"] ?? config["ANTHROPIC_API_KEY"] ?? "";
    var modelId = config["Llm:Providers:Anthropic:ModelId"] ?? "claude-3-5-sonnet-20241022";
    var maxTokens = config.GetValue("Llm:Providers:Anthropic:MaxTokens", 1024);
    var anthropicClient = new AnthropicClient(apiKey);
    return new AnthropicProvider(anthropicClient, modelId, maxTokens);
});

var defaultProvider = config["Llm:DefaultProvider"] ?? OpenAiProvider.Key;
builder.Services.AddSingleton<ILlmProviderFactory>(sp =>
    new KeyedLlmProviderFactory((IKeyedServiceProvider)sp, defaultProvider));

builder.Services.AddScoped<IChatOrchestrator, ChatOrchestrator>();

builder.Services.AddFastEndpoints();

var app = builder.Build();

app.UseFastEndpoints();
app.Run();