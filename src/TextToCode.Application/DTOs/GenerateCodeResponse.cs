using TextToCode.Core.Enums;

namespace TextToCode.Application.DTOs;

public sealed record GenerateCodeResponse(
    PipelineStatus Status,
    string? GeneratedCode,
    string? ConsoleOutput,
    int Attempts,
    TimeSpan? Duration,
    IReadOnlyList<string> DiagnosticHistory);
