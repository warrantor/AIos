using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using AIos.Sdk.Chat;
using Spectre.Console;
using Spectre.Console.Cli;

namespace AIos.Cli.Commands;

public sealed class ChatCommand : AsyncCommand {
    private const string GatewayUrl = "https://aios-gateway.dev.localhost:7218";
    private static readonly JsonSerializerOptions JsonOpts = new(JsonSerializerDefaults.Web);

    public override async Task<int> ExecuteAsync(CommandContext context, CancellationToken cancellationToken) {
        using var http = new HttpClient();
        http.BaseAddress = new Uri(GatewayUrl);

        var history = new List<ChatMessage>();

        AnsiConsole.MarkupLine("[bold green]AIos Chat[/] [dim](type 'exit' to quit)[/]");
        AnsiConsole.WriteLine();

        while (true) {
            var input = AnsiConsole.Ask<string>("[bold cyan]You:[/]");

            if (input.Equals("exit", StringComparison.OrdinalIgnoreCase)) {
                break;
            }

            history.Add(new ChatMessage(ChatRole.User, input));

            var request = new ChatRequest(history);

            using var response = await http.PostAsJsonAsync("/chat/stream", request, JsonOpts, cancellationToken);

            response.EnsureSuccessStatusCode();

            AnsiConsole.Markup("[bold yellow]AIos:[/] ");

            var reply = await ReadStreamAsync(response, cancellationToken);

            AnsiConsole.WriteLine();
            AnsiConsole.WriteLine();

            history.Add(new ChatMessage(ChatRole.Assistant, reply));
        }

        return 0;
    }

    private static async Task<string> ReadStreamAsync(HttpResponseMessage response, CancellationToken ct) {
        var sb = new StringBuilder();

        await using var stream = await response.Content.ReadAsStreamAsync(ct);
        using var reader = new StreamReader(stream);

        while (await reader.ReadLineAsync(ct) is { } line) {
            if (!line.StartsWith("data: ")) {
                continue;
            }

            var data = line["data: ".Length..];

            if (data == "[DONE]") {
                break;
            }

            Console.Write(data); // raw write for streaming feel
            sb.Append(data);
        }

        return sb.ToString();
    }
}