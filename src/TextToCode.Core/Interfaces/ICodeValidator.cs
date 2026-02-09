using TextToCode.Core.Entities;

namespace TextToCode.Core.Interfaces;

public interface ICodeValidator
{
    ValidationResult Validate(string code);
}
