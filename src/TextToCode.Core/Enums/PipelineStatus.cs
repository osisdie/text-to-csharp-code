namespace TextToCode.Core.Enums;

public enum PipelineStatus
{
    Pending,
    Generating,
    Validating,
    Compiling,
    Executing,
    SelfHealing,
    Completed,
    Failed
}
