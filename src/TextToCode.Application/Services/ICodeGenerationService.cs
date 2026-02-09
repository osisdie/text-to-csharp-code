using TextToCode.Application.DTOs;

namespace TextToCode.Application.Services;

public interface ICodeGenerationService
{
    Task<GenerateCodeResponse> GenerateAsync(
        GenerateCodeRequest request,
        IProgress<PipelineProgressUpdate>? progress = null,
        CancellationToken cancellationToken = default);
}
