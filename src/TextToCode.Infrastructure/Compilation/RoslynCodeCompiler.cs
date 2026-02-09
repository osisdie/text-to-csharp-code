using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using TextToCode.Core.Entities;
using TextToCode.Core.Interfaces;
using TextToCode.Core.ValueObjects;

namespace TextToCode.Infrastructure.Compilation;

public sealed class RoslynCodeCompiler : ICodeCompiler
{
    private static readonly CSharpParseOptions ParseOptions = new(LanguageVersion.Latest);

    private static readonly CSharpCompilationOptions CompilationOptions = new(
        OutputKind.ConsoleApplication,
        optimizationLevel: OptimizationLevel.Release,
        allowUnsafe: false);

    public CompilationResult Compile(string code)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(code, ParseOptions);

        var compilation = CSharpCompilation.Create(
            assemblyName: $"DynamicAssembly_{Guid.NewGuid():N}",
            syntaxTrees: [syntaxTree],
            references: CompilationReferenceResolver.GetDefaultReferences(),
            options: CompilationOptions);

        using var ms = new MemoryStream();
        var emitResult = compilation.Emit(ms);

        var diagnostics = emitResult.Diagnostics
            .Where(d => d.Severity >= DiagnosticSeverity.Warning)
            .Select(d =>
            {
                var lineSpan = d.Location.GetLineSpan();
                return new CompilerDiagnostic(
                    d.Id,
                    d.GetMessage(),
                    d.Severity.ToString(),
                    lineSpan.StartLinePosition.Line + 1,
                    lineSpan.StartLinePosition.Character + 1);
            })
            .ToList();

        if (emitResult.Success)
        {
            return CompilationResult.Succeeded(ms.ToArray(), diagnostics);
        }

        return CompilationResult.Failed(diagnostics);
    }
}
