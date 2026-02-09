using Microsoft.AspNetCore.Mvc;
using TextToCode.Application.DTOs;
using TextToCode.Application.Services;

namespace TextToCode.WebApi.Controllers;

[ApiController]
[Route("api/codegen")]
public sealed class CodeGenerationController : ControllerBase
{
    private readonly ICodeGenerationService _service;

    public CodeGenerationController(ICodeGenerationService service)
    {
        _service = service;
    }

    [HttpPost("generate")]
    [ProducesResponseType(typeof(GenerateCodeResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Generate(
        [FromBody] GenerateCodeRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Prompt))
            return BadRequest(new { error = "Prompt is required." });

        var response = await _service.GenerateAsync(request, cancellationToken: cancellationToken);

        return response.Status == Core.Enums.PipelineStatus.Completed
            ? Ok(response)
            : UnprocessableEntity(response);
    }
}
