using Application.Users.Commands.CreateUserFromWebhook;
using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

public record ClerkWebhookData(string? Id);
public record ClerkWebhookPayload(string? Type, ClerkWebhookData? Data);

[ApiController]
[ApiVersion("1.0")]
[Route("v{version:apiVersion}/create-user")]
[Route("create-user")]
[AllowAnonymous]
public class UserController : ControllerBase
{
    private readonly IMediator _mediator;

    public UserController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    public async Task<IActionResult> CreateUser(ClerkWebhookPayload payload, CancellationToken ct)
    {
        var user = await _mediator.Send(
            new CreateUserFromWebhookCommand(payload.Type, payload.Data?.Id), ct);
        return Ok(new { message = "User created successfully", data = user });
    }
}
