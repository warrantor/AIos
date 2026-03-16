using AIos.Cli.Commands;
using Spectre.Console.Cli;

var app = new CommandApp<ChatCommand>();
return app.Run(args);