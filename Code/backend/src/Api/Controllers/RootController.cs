using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("v{version:apiVersion}/")]
[Route("/")]
[AllowAnonymous]
public class RootController : ControllerBase
{
    [HttpGet]
    public IActionResult Get() => Ok(new { message = "Welcome to My engineering API!" });
}
