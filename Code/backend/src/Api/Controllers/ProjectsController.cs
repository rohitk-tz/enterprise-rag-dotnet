using Application.Projects.Queries.GetProjects;
using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("v{version:apiVersion}/api/projects")]
[Route("api/projects")]
[Authorize]
public class ProjectsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ProjectsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> GetProjects(CancellationToken ct)
    {
        var projects = await _mediator.Send(new GetProjectsQuery(), ct);
        return Ok(new { message = "Projects retrieved successfully", data = projects });
    }

    [HttpGet("{projectId:guid}")]
    public async Task<IActionResult> GetProjectById(Guid projectId, CancellationToken ct)
    {
        var project = await _mediator.Send(new Application.Projects.Queries.GetProjectById.GetProjectByIdQuery(projectId), ct);
        return Ok(new { message = "Project details retrieved successfully", data = project });
    }

    [HttpGet("{projectId:guid}/chats")]
    public async Task<IActionResult> GetProjectChats(Guid projectId, CancellationToken ct)
    {
        var chats = await _mediator.Send(new Application.Projects.Queries.GetProjectChats.GetProjectChatsQuery(projectId), ct);
        return Ok(new { message = "Chats retrieved successfully", data = chats });
    }

    [HttpGet("{projectId:guid}/settings")]
    public async Task<IActionResult> GetProjectSettings(Guid projectId, CancellationToken ct)
    {
        var settings = await _mediator.Send(new Application.Projects.Queries.GetProjectSettings.GetProjectSettingsQuery(projectId), ct);
        return Ok(new { message = "Project settings retrieved successfully", data = settings });
    }

    public record CreateProjectRequest(string Name, string? Description);

    [HttpPost]
    public async Task<IActionResult> CreateProject(CreateProjectRequest request, CancellationToken ct)
    {
        var project = await _mediator.Send(
            new Application.Projects.Commands.CreateProject.CreateProjectCommand(request.Name, request.Description ?? ""), ct);
        return Ok(new { message = "Project created successfully", data = new[] { project } });
    }

    [HttpDelete("{projectId:guid}")]
    public async Task<IActionResult> DeleteProject(Guid projectId, CancellationToken ct)
    {
        await _mediator.Send(new Application.Projects.Commands.DeleteProject.DeleteProjectCommand(projectId), ct);
        return Ok(new { message = "Project deleted successfully" });
    }

    public record UpdateProjectSettingsRequest(
        string EmbeddingModel, string RagStrategy, string AgentType, int ChunksPerSearch,
        int FinalContextSize, decimal SimilarityThreshold, int NumberOfQueries,
        bool RerankingEnabled, string RerankingModel, decimal VectorWeight, decimal KeywordWeight);

    [HttpPut("{projectId:guid}/settings")]
    public async Task<IActionResult> UpdateProjectSettings(Guid projectId, UpdateProjectSettingsRequest request, CancellationToken ct)
    {
        var settings = await _mediator.Send(
            new Application.Projects.Commands.UpdateProjectSettings.UpdateProjectSettingsCommand(
                projectId, request.EmbeddingModel, request.RagStrategy, request.AgentType,
                request.ChunksPerSearch, request.FinalContextSize, request.SimilarityThreshold,
                request.NumberOfQueries, request.RerankingEnabled, request.RerankingModel,
                request.VectorWeight, request.KeywordWeight), ct);
        return Ok(new { message = "Project settings updated successfully", data = settings });
    }

    public record SendMessageRequest(string Content);

    [HttpPost("{projectId:guid}/chats/{chatId:guid}/messages")]
    public async Task<IActionResult> SendMessage(Guid projectId, Guid chatId, SendMessageRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(
            new Application.Chats.Commands.SendMessage.SendMessageCommand(projectId, chatId, request.Content), ct);
        return Ok(new { message = "Messages sent successfully", data = new { userMessage = result.UserMessage, aiMessage = result.AiMessage } });
    }
}
