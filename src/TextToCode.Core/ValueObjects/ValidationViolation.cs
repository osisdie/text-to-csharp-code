using TextToCode.Core.Enums;

namespace TextToCode.Core.ValueObjects;

public sealed record ValidationViolation(
    string Rule,
    string Description,
    ValidationSeverity Severity,
    int? Line = null,
    int? Column = null);
