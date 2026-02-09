using TextToCode.Core.Enums;

namespace TextToCode.Core.Entities;

public sealed class PipelineContext
{
    public string UserPrompt { get; }
    public PipelineStatus Status { get; private set; } = PipelineStatus.Pending;
    public string? GeneratedCode { get; private set; }
    public ValidationResult? Validation { get; private set; }
    public CompilationResult? Compilation { get; private set; }
    public ExecutionResult? Execution { get; private set; }
    public int AttemptNumber { get; private set; }
    public List<string> DiagnosticHistory { get; } = [];
    public DateTime StartedAt { get; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; private set; }

    public PipelineContext(string userPrompt)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userPrompt);
        UserPrompt = userPrompt;
    }

    public void SetGenerating(int attempt)
    {
        Status = PipelineStatus.Generating;
        AttemptNumber = attempt;
    }

    public void SetGeneratedCode(string code)
    {
        GeneratedCode = code;
    }

    public void SetValidating() => Status = PipelineStatus.Validating;

    public void SetValidation(ValidationResult result) => Validation = result;

    public void SetCompiling() => Status = PipelineStatus.Compiling;

    public void SetCompilation(CompilationResult result) => Compilation = result;

    public void SetExecuting() => Status = PipelineStatus.Executing;

    public void SetExecution(ExecutionResult result) => Execution = result;

    public void SetSelfHealing(string diagnosticInfo)
    {
        Status = PipelineStatus.SelfHealing;
        DiagnosticHistory.Add(diagnosticInfo);
    }

    public void SetCompleted()
    {
        Status = PipelineStatus.Completed;
        CompletedAt = DateTime.UtcNow;
    }

    public void SetFailed()
    {
        Status = PipelineStatus.Failed;
        CompletedAt = DateTime.UtcNow;
    }

    public TimeSpan? TotalDuration => CompletedAt.HasValue ? CompletedAt.Value - StartedAt : null;
}
