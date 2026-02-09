using TextToCode.Core.Enums;
using TextToCode.Core.ValueObjects;

namespace TextToCode.Core.Entities;

public sealed class ValidationResult
{
    public bool IsValid { get; }
    public IReadOnlyList<ValidationViolation> Violations { get; }

    private ValidationResult(bool isValid, IReadOnlyList<ValidationViolation> violations)
    {
        IsValid = isValid;
        Violations = violations;
    }

    public static ValidationResult Valid(IReadOnlyList<ValidationViolation>? warnings = null) =>
        new(true, warnings ?? []);

    public static ValidationResult Invalid(IReadOnlyList<ValidationViolation> violations) =>
        new(false, violations);

    public bool HasErrors => Violations.Any(v => v.Severity == ValidationSeverity.Error);

    public string GetViolationsSummary() =>
        string.Join(Environment.NewLine, Violations.Select(v => $"[{v.Severity}] {v.Rule}: {v.Description}" +
            (v.Line.HasValue ? $" (line {v.Line})" : "")));
}
