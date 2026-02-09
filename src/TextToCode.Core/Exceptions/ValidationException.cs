using TextToCode.Core.ValueObjects;

namespace TextToCode.Core.Exceptions;

public class ValidationException : CodeGenerationException
{
    public IReadOnlyList<ValidationViolation> Violations { get; }

    public ValidationException(string message, IReadOnlyList<ValidationViolation> violations)
        : base(message)
    {
        Violations = violations;
    }
}
