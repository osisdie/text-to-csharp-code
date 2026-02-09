namespace TextToCode.Application.Pipelines;

public sealed class PipelineOptions
{
    public const string SectionName = "Pipeline";

    public int MaxRetries { get; set; } = 3;
    public int ExecutionTimeoutSeconds { get; set; } = 10;
}
