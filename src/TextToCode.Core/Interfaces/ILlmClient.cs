namespace TextToCode.Core.Interfaces;

public interface ILlmClient
{
    Task<string> GenerateCodeAsync(string systemPrompt, string userPrompt, CancellationToken cancellationToken = default);
    Task<string> DiagnoseAndFixAsync(string systemPrompt, string code, string error, CancellationToken cancellationToken = default);
}
