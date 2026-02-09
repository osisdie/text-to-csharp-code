using TextToCode.Core.ValueObjects;

namespace TextToCode.Core.Entities;

public sealed class CompilationResult
{
    public bool Success { get; }
    public byte[]? AssemblyBytes { get; }
    public IReadOnlyList<CompilerDiagnostic> Diagnostics { get; }

    private CompilationResult(bool success, byte[]? assemblyBytes, IReadOnlyList<CompilerDiagnostic> diagnostics)
    {
        Success = success;
        AssemblyBytes = assemblyBytes;
        Diagnostics = diagnostics;
    }

    public static CompilationResult Succeeded(byte[] assemblyBytes, IReadOnlyList<CompilerDiagnostic> diagnostics) =>
        new(true, assemblyBytes, diagnostics);

    public static CompilationResult Failed(IReadOnlyList<CompilerDiagnostic> diagnostics) =>
        new(false, null, diagnostics);

    public string GetDiagnosticsSummary() =>
        string.Join(Environment.NewLine, Diagnostics.Select(d => $"{d.Severity} {d.Id} ({d.Line},{d.Column}): {d.Message}"));
}
