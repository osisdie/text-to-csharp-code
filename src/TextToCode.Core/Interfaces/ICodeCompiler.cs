using TextToCode.Core.Entities;

namespace TextToCode.Core.Interfaces;

public interface ICodeCompiler
{
    CompilationResult Compile(string code);
}
