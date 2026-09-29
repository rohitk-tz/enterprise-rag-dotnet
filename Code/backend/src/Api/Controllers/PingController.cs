using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("v{version:apiVersion}/api/ping")]
[Route("api/ping")]
public class PingController : ControllerBase
{
    [HttpGet]
    [Authorize]
    public IActionResult Get() => Ok(new { message = "pong" });

    [HttpGet("throws-not-found")]
    [AllowAnonymous]
    public IActionResult ThrowsNotFound() =>
        throw new Application.Common.Exceptions.NotFoundException("Project not found.");

    [HttpGet("throws-forbidden")]
    [AllowAnonymous]
    public IActionResult ThrowsForbidden() =>
        throw new Application.Common.Exceptions.ForbiddenException("Not your project.");
}
