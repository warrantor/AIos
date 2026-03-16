using AIos.Core.Application.Chat;
using FastEndpoints;
using Microsoft.Extensions.AI;
using OpenAI;

var builder = WebApplication.CreateBuilder(args);

// var openAiClient = new OpenAIClient(builder.Configuration["AI:ApiKey"]!);
var openAiClient =
    new OpenAIClient(
        "...");

// Register as default (non-keyed) IChatClient for ChatOrchestrator and ChatStreamEndpoint
// ReSharper disable once RedundantTypeArgumentsOfMethod
builder.Services.AddSingleton<IChatClient>(openAiClient.GetChatClient("gpt-4o").AsIChatClient());

builder.Services.AddScoped<IChatOrchestrator, ChatOrchestrator>();

builder.Services.AddFastEndpoints();

var app = builder.Build();

app.UseFastEndpoints();
app.Run();