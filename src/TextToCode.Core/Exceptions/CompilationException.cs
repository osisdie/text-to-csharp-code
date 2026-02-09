using TextToCode.Core.ValueObjects;

namespace TextToCode.Core.Exceptions;

public class CompilationException : CodeGenerationException
{
    public IReadOnlyList<CompilerDiagnostic> Diagnostics { get; }

    public CompilationException(string message, IReadOnlyList<CompilerDiagnostic> diagnostics)
        : base(message)
    {
        Diagnostics = diagnostics;
    }
}
