using TextToCode.Core.Entities;

namespace TextToCode.Core.Interfaces;

public interface ICodeExecutor
{
    Task<ExecutionResult> ExecuteAsync(byte[] assemblyBytes, CancellationToken cancellationToken = default);
}
