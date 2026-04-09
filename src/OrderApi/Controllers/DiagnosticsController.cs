using Microsoft.AspNetCore.Mvc;
using OrderApi.Middleware;
using OrderApi.Services.Exceptions;

namespace OrderApi.Controllers;

[ApiController]
[Route("api/diagnostics")]
public class DiagnosticsController : ControllerBase
{
    [HttpGet("ping")]
    public IActionResult Ping()
    {
        return Ok(new
        {
            message = "pong",
            correlationId = HttpContext.GetCorrelationId()
        });
    }

    [HttpGet("fail")]
    public IActionResult Fail()
    {
        throw new NotFoundException("Demo not found.");
    }
}
