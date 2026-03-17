using AIos.Core.Application.Chat;
using AIos.Sdk.Chat;
using FastEndpoints;

namespace AIos.Gateway.Endpoints.Chat;

public sealed class ChatStreamEndpoint(IChatOrchestrator orchestrator) : Endpoint<ChatRequest>
{
    public override void Configure()
    {
        Post("/chat/stream");
        AllowAnonymous();
    }

    public override async Task HandleAsync(ChatRequest req, CancellationToken ct)
    {
        ct = HttpContext.RequestAborted;
        var history = req.History;

        HttpContext.Response.ContentType = "text/event-stream";
        HttpContext.Response.Headers.CacheControl = "no-cache";
        HttpContext.Response.Headers.Connection = "keep-alive";

        await foreach (var token in orchestrator.StreamAsync(history, req.ProviderId, ct))
        {
            await HttpContext.Response.WriteAsync($"data: {token}\n\n", ct);
            await HttpContext.Response.Body.FlushAsync(ct);
        }

        await HttpContext.Response.WriteAsync("data: [DONE]\n\n", ct);
        await HttpContext.Response.Body.FlushAsync(ct);
    }
}
