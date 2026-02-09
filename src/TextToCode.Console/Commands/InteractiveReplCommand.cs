using System.CommandLine;
using System.CommandLine.Invocation;
using Spectre.Console;
using TextToCode.Application.DTOs;
using TextToCode.Console.Display;

namespace TextToCode.Console.Commands;

public sealed class InteractiveReplCommand : Command
{
    public InteractiveReplCommand() : base("repl", "Start interactive REPL mode")
    {
        this.SetHandler(HandleAsync);
    }

    private static async Task HandleAsync(InvocationContext context)
    {
        var ct = context.GetCancellationToken();

        var service = GenerateCommandState.Service
            ?? throw new InvalidOperationException("CodeGenerationService not configured.");

        ConsoleRenderer.RenderWelcome();

        while (!ct.IsCancellationRequested)
        {
            var prompt = AnsiConsole.Ask<string>("[bold cyan]>[/]");

            if (string.IsNullOrWhiteSpace(prompt))
                continue;

            if (prompt.Equals("exit", StringComparison.OrdinalIgnoreCase) ||
                prompt.Equals("quit", StringComparison.OrdinalIgnoreCase))
            {
                AnsiConsole.MarkupLine("[dim]Goodbye![/]");
                break;
            }

            if (prompt.Equals("clear", StringComparison.OrdinalIgnoreCase))
            {
                AnsiConsole.Clear();
                ConsoleRenderer.RenderWelcome();
                continue;
            }

            var progress = new Progress<PipelineProgressUpdate>(ConsoleRenderer.RenderProgressUpdate);

            try
            {
                var response = await service.GenerateAsync(new GenerateCodeRequest(prompt), progress, ct);
                ConsoleRenderer.RenderResult(response);
            }
            catch (OperationCanceledException)
            {
                AnsiConsole.MarkupLine("[yellow]Operation cancelled.[/]");
            }
            catch (Exception ex)
            {
                ConsoleRenderer.RenderError(ex.Message);
            }
        }
    }
}
