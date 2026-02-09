using System.CommandLine;
using System.CommandLine.Invocation;
using TextToCode.Application.DTOs;
using TextToCode.Application.Services;
using TextToCode.Console.Display;

namespace TextToCode.Console.Commands;

public sealed class GenerateCommand : Command
{
    private readonly Argument<string> _promptArg = new("prompt", "The natural language description of the code to generate");

    public GenerateCommand() : base("generate", "Generate C# code from a natural language prompt")
    {
        AddArgument(_promptArg);

        this.SetHandler(HandleAsync);
    }

    private async Task HandleAsync(InvocationContext context)
    {
        var prompt = context.ParseResult.GetValueForArgument(_promptArg);
        var ct = context.GetCancellationToken();

        var service = GenerateCommandState.Service
            ?? throw new InvalidOperationException("CodeGenerationService not configured.");

        var progress = new Progress<PipelineProgressUpdate>(ConsoleRenderer.RenderProgressUpdate);

        try
        {
            var response = await service.GenerateAsync(new GenerateCodeRequest(prompt), progress, ct);
            ConsoleRenderer.RenderResult(response);
        }
        catch (Exception ex)
        {
            ConsoleRenderer.RenderError(ex.Message);
        }
    }
}

internal static class GenerateCommandState
{
    public static ICodeGenerationService? Service { get; set; }
}
