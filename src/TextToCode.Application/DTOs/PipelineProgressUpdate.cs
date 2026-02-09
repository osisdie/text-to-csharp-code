using TextToCode.Core.Enums;

namespace TextToCode.Application.DTOs;

public sealed record PipelineProgressUpdate(
    PipelineStatus Status,
    int Attempt,
    string Message);
