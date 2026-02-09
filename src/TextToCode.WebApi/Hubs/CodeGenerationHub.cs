using Microsoft.AspNetCore.SignalR;
using TextToCode.Application.DTOs;
using TextToCode.Application.Services;

namespace TextToCode.WebApi.Hubs;

public sealed class CodeGenerationHub : Hub
{
    private readonly ICodeGenerationService _service;

    public CodeGenerationHub(ICodeGenerationService service)
    {
        _service = service;
    }

    public async Task GenerateCode(string prompt)
    {
        var progress = new Progress<PipelineProgressUpdate>(async update =>
        {
            await Clients.Caller.SendAsync("ProgressUpdate", update);
        });

        var response = await _service.GenerateAsync(
            new GenerateCodeRequest(prompt),
            progress,
            Context.ConnectionAborted);

        await Clients.Caller.SendAsync("GenerationComplete", response);
    }
}
