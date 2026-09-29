using Application.Chats.Commands.CreateChat;
using Application.Chats.Commands.DeleteChat;
using Application.Chats.Queries.GetChatWithMessages;
using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("v{version:apiVersion}/api/chats")]
[Route("api/chats")]
[Authorize]
public class ChatsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ChatsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    public record CreateChatRequest(Guid ProjectId, string Title);

    [HttpPost]
    public async Task<IActionResult> CreateChat(CreateChatRequest request, CancellationToken ct)
    {
        var chat = await _mediator.Send(
            new CreateChatCommand(request.ProjectId, request.Title), ct);
        return Ok(new { message = "Chat created successfully", data = chat });
    }

    [HttpGet("{chatId:guid}")]
    public async Task<IActionResult> GetChat(Guid chatId, CancellationToken ct)
    {
        var chat = await _mediator.Send(new GetChatWithMessagesQuery(chatId), ct);
        return Ok(new { message = "Chat retrieved successfully", data = chat });
    }

    [HttpDelete("{chatId:guid}")]
    public async Task<IActionResult> DeleteChat(Guid chatId, CancellationToken ct)
    {
        await _mediator.Send(new DeleteChatCommand(chatId), ct);
        return Ok(new { message = "Chat deleted successfully" });
    }
}
