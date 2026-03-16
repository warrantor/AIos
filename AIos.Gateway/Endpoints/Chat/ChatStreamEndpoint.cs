using AIos.Core.Infrastructure;
using AIos.Sdk.Chat;
using FastEndpoints;
using Microsoft.Extensions.AI;
using ChatMessage = Microsoft.Extensions.AI.ChatMessage;

namespace AIos.Gateway.Endpoints.Chat;

public sealed class ChatStreamEndpoint(IChatClient chatClient) : Endpoint<ChatRequest> {
    public override void Configure() {
        Post("/chat/stream");
        AllowAnonymous();
    }

    public override async Task HandleAsync(ChatRequest req, CancellationToken ct) {
        var history = req.History
            .Select(m => new ChatMessage(m.Role.ToExtensionsAiRole(), m.Content))
            .ToList();

        HttpContext.Response.ContentType = "text/event-stream";
        HttpContext.Response.Headers.CacheControl = "no-cache";
        HttpContext.Response.Headers.Connection = "keep-alive";

        await foreach (var update in chatClient.GetStreamingResponseAsync(history, cancellationToken: ct)) {
            if (update.Text is not { Length: > 0 } token) {
                continue;
            }

            await HttpContext.Response.WriteAsync($"data: {token}\n\n", ct);
            await HttpContext.Response.Body.FlushAsync(ct);
        }

        // Signal end of stream
        await HttpContext.Response.WriteAsync("data: [DONE]\n\n", ct);
        await HttpContext.Response.Body.FlushAsync(ct);
    }
}