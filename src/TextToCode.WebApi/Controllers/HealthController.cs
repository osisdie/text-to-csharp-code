using Microsoft.AspNetCore.Mvc;

namespace TextToCode.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class HealthController : ControllerBase
{
    [HttpGet]
    public IActionResult Get() => Ok(new
    {
        status = "healthy",
        timestamp = DateTime.UtcNow,
        version = typeof(HealthController).Assembly.GetName().Version?.ToString() ?? "1.0.0"
    });
}
