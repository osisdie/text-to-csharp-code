using TextToCode.Application.DTOs;
using TextToCode.Application.Pipelines;
using TextToCode.Core.Interfaces;

namespace TextToCode.Application.Services;

public sealed class CodeGenerationService : ICodeGenerationService
{
    private readonly CodeGenerationPipeline _pipeline;
    private readonly IPromptTemplateFactory _promptFactory;

    public CodeGenerationService(CodeGenerationPipeline pipeline, IPromptTemplateFactory promptFactory)
    {
        _pipeline = pipeline;
        _promptFactory = promptFactory;
    }

    public async Task<GenerateCodeResponse> GenerateAsync(
        GenerateCodeRequest request,
        IProgress<PipelineProgressUpdate>? progress = null,
        CancellationToken cancellationToken = default)
    {
        var codeGenTemplate = _promptFactory.GetCodeGenerationTemplate();
        var selfHealTemplate = _promptFactory.GetSelfHealingTemplate();

        var systemPrompt = codeGenTemplate.Render(new Dictionary<string, string>
        {
            ["USER_PROMPT"] = request.Prompt
        });

        var selfHealingPrompt = selfHealTemplate.Render(new Dictionary<string, string>
        {
            ["CODE"] = string.Empty,
            ["ERROR"] = string.Empty
        });

        var context = await _pipeline.ExecuteAsync(
            request.Prompt,
            systemPrompt,
            selfHealingPrompt,
            progress,
            cancellationToken);

        return new GenerateCodeResponse(
            context.Status,
            context.GeneratedCode,
            context.Execution?.ConsoleOutput,
            context.AttemptNumber,
            context.TotalDuration,
            context.DiagnosticHistory);
    }
}
