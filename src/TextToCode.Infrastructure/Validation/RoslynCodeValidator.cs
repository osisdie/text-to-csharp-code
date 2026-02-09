using Microsoft.CodeAnalysis.CSharp;
using TextToCode.Core.Entities;
using TextToCode.Core.Enums;
using TextToCode.Core.Interfaces;
using TextToCode.Core.ValueObjects;

namespace TextToCode.Infrastructure.Validation;

public sealed class RoslynCodeValidator : ICodeValidator
{
    public ValidationResult Validate(string code)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            return ValidationResult.Invalid([
                new ValidationViolation("EmptyCode", "Generated code is empty", ValidationSeverity.Error)
            ]);
        }

        var tree = CSharpSyntaxTree.ParseText(code);
        var root = tree.GetRoot();

        // Check for syntax errors
        var syntaxDiagnostics = tree.GetDiagnostics()
            .Where(d => d.Severity == Microsoft.CodeAnalysis.DiagnosticSeverity.Error)
            .Select(d =>
            {
                var lineSpan = d.Location.GetLineSpan();
                return new ValidationViolation(
                    $"SyntaxError:{d.Id}",
                    d.GetMessage(),
                    ValidationSeverity.Error,
                    lineSpan.StartLinePosition.Line + 1,
                    lineSpan.StartLinePosition.Character + 1);
            })
            .ToList();

        if (syntaxDiagnostics.Count > 0)
        {
            return ValidationResult.Invalid(syntaxDiagnostics);
        }

        // Walk the AST for dangerous APIs
        var walker = new DangerousApiWalker();
        walker.Visit(root);

        if (walker.Violations.Count > 0)
        {
            return ValidationResult.Invalid(walker.Violations.ToList());
        }

        return ValidationResult.Valid();
    }
}
