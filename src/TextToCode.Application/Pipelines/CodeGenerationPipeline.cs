using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TextToCode.Application.DTOs;
using TextToCode.Core.Entities;
using TextToCode.Core.Enums;
using TextToCode.Core.Interfaces;

namespace TextToCode.Application.Pipelines;

public sealed class CodeGenerationPipeline
{
    private readonly ILlmClient _llmClient;
    private readonly ICodeValidator _validator;
    private readonly ICodeCompiler _compiler;
    private readonly ICodeExecutor _executor;
    private readonly PipelineOptions _options;
    private readonly ILogger<CodeGenerationPipeline> _logger;

    public CodeGenerationPipeline(
        ILlmClient llmClient,
        ICodeValidator validator,
        ICodeCompiler compiler,
        ICodeExecutor executor,
        IOptions<PipelineOptions> options,
        ILogger<CodeGenerationPipeline> logger)
    {
        _llmClient = llmClient;
        _validator = validator;
        _compiler = compiler;
        _executor = executor;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<PipelineContext> ExecuteAsync(
        string userPrompt,
        string systemPrompt,
        string selfHealingPrompt,
        IProgress<PipelineProgressUpdate>? progress = null,
        CancellationToken cancellationToken = default)
    {
        var context = new PipelineContext(userPrompt);
        var maxAttempts = _options.MaxRetries + 1;

        for (var attempt = 1; attempt <= maxAttempts; attempt++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            // Generate
            context.SetGenerating(attempt);
            Report(progress, PipelineStatus.Generating, attempt, attempt == 1
                ? "Generating code..."
                : $"Self-healing attempt {attempt}/{maxAttempts}...");

            try
            {
                string code;
                if (attempt == 1)
                {
                    code = await _llmClient.GenerateCodeAsync(systemPrompt, userPrompt, cancellationToken);
                }
                else
                {
                    var lastError = context.DiagnosticHistory[^1];
                    code = await _llmClient.DiagnoseAndFixAsync(selfHealingPrompt, context.GeneratedCode!, lastError, cancellationToken);
                }

                context.SetGeneratedCode(code);
                _logger.LogDebug("Attempt {Attempt}: Generated {Length} chars", attempt, code.Length);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "LLM generation failed on attempt {Attempt}", attempt);
                context.SetSelfHealing($"LLM error: {ex.Message}");
                continue;
            }

            // Validate
            context.SetValidating();
            Report(progress, PipelineStatus.Validating, attempt, "Validating code safety...");

            var validation = _validator.Validate(context.GeneratedCode!);
            context.SetValidation(validation);

            if (!validation.IsValid)
            {
                var summary = validation.GetViolationsSummary();
                _logger.LogWarning("Validation failed on attempt {Attempt}: {Summary}", attempt, summary);
                context.SetSelfHealing($"Validation failed:\n{summary}");
                Report(progress, PipelineStatus.SelfHealing, attempt, "Validation failed, retrying...");
                continue;
            }

            // Compile
            context.SetCompiling();
            Report(progress, PipelineStatus.Compiling, attempt, "Compiling with Roslyn...");

            var compilation = _compiler.Compile(context.GeneratedCode!);
            context.SetCompilation(compilation);

            if (!compilation.Success)
            {
                var summary = compilation.GetDiagnosticsSummary();
                _logger.LogWarning("Compilation failed on attempt {Attempt}: {Summary}", attempt, summary);
                context.SetSelfHealing($"Compilation errors:\n{summary}");
                Report(progress, PipelineStatus.SelfHealing, attempt, "Compilation failed, retrying...");
                continue;
            }

            // Execute
            context.SetExecuting();
            Report(progress, PipelineStatus.Executing, attempt, "Executing in sandbox...");

            var execution = await _executor.ExecuteAsync(compilation.AssemblyBytes!, cancellationToken);
            context.SetExecution(execution);

            if (!execution.Success)
            {
                var errorInfo = $"Runtime error ({execution.ExceptionType}): {execution.ErrorMessage}";
                _logger.LogWarning("Execution failed on attempt {Attempt}: {Error}", attempt, errorInfo);
                context.SetSelfHealing(errorInfo);
                Report(progress, PipelineStatus.SelfHealing, attempt, "Runtime error, retrying...");
                continue;
            }

            // Success
            context.SetCompleted();
            Report(progress, PipelineStatus.Completed, attempt, "Code executed successfully!");
            _logger.LogInformation("Pipeline completed successfully on attempt {Attempt}", attempt);
            return context;
        }

        // All attempts exhausted
        context.SetFailed();
        Report(progress, PipelineStatus.Failed, maxAttempts, "All retry attempts exhausted.");
        _logger.LogError("Pipeline failed after {MaxAttempts} attempts", maxAttempts);
        return context;
    }

    private static void Report(IProgress<PipelineProgressUpdate>? progress, PipelineStatus status, int attempt, string message) =>
        progress?.Report(new PipelineProgressUpdate(status, attempt, message));
}
