using Spectre.Console;
using TextToCode.Application.DTOs;
using TextToCode.Core.Enums;

namespace TextToCode.Console.Display;

public static class ConsoleRenderer
{
    public static void RenderWelcome()
    {
        AnsiConsole.Write(new FigletText("TextToCode").Color(Color.Cyan1));
        AnsiConsole.MarkupLine("[dim]AI-powered C# code generation with self-healing pipeline[/]");
        AnsiConsole.MarkupLine("[dim]Type a natural language prompt, or 'exit' to quit.[/]");
        AnsiConsole.WriteLine();
    }

    public static void RenderProgressUpdate(PipelineProgressUpdate update)
    {
        var (icon, color) = update.Status switch
        {
            PipelineStatus.Generating => ("🤖", "cyan"),
            PipelineStatus.Validating => ("🔍", "yellow"),
            PipelineStatus.Compiling => ("🔨", "blue"),
            PipelineStatus.Executing => ("▶️", "green"),
            PipelineStatus.SelfHealing => ("🔧", "orange1"),
            PipelineStatus.Completed => ("✅", "green"),
            PipelineStatus.Failed => ("❌", "red"),
            _ => ("⏳", "grey")
        };

        AnsiConsole.MarkupLine($"  {icon} [{color}]{update.Message.EscapeMarkup()}[/]");
    }

    public static void RenderResult(GenerateCodeResponse response)
    {
        AnsiConsole.WriteLine();

        if (response.Status == PipelineStatus.Completed)
        {
            // Generated code panel
            if (!string.IsNullOrWhiteSpace(response.GeneratedCode))
            {
                var codePanel = new Panel(response.GeneratedCode.EscapeMarkup())
                {
                    Header = new PanelHeader("[bold cyan] Generated Code [/]"),
                    Border = BoxBorder.Rounded,
                    BorderStyle = new Style(Color.Cyan1),
                    Padding = new Padding(1, 0)
                };
                AnsiConsole.Write(codePanel);
            }

            // Output panel
            if (!string.IsNullOrWhiteSpace(response.ConsoleOutput))
            {
                var outputPanel = new Panel(response.ConsoleOutput.EscapeMarkup())
                {
                    Header = new PanelHeader("[bold green] Output [/]"),
                    Border = BoxBorder.Rounded,
                    BorderStyle = new Style(Color.Green),
                    Padding = new Padding(1, 0)
                };
                AnsiConsole.Write(outputPanel);
            }

            // Metadata
            AnsiConsole.MarkupLine($"[dim]Attempts: {response.Attempts} | Duration: {response.Duration?.TotalSeconds:F1}s[/]");
        }
        else
        {
            AnsiConsole.MarkupLine("[bold red]Pipeline failed.[/]");

            if (response.DiagnosticHistory.Count > 0)
            {
                AnsiConsole.MarkupLine("[dim]Diagnostic history:[/]");
                foreach (var diag in response.DiagnosticHistory)
                {
                    AnsiConsole.MarkupLine($"  [red]• {diag.EscapeMarkup()}[/]");
                }
            }
        }

        AnsiConsole.WriteLine();
    }

    public static void RenderError(string message)
    {
        AnsiConsole.MarkupLine($"[bold red]Error:[/] {message.EscapeMarkup()}");
    }
}
