namespace TextToCode.Core.ValueObjects;

public sealed record CompilerDiagnostic(
    string Id,
    string Message,
    string Severity,
    int Line,
    int Column);
