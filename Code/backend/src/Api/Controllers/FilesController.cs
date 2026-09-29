using Application.Files.Queries.GetDocumentChunks;
using Application.Files.Queries.GetProjectFiles;
using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("v{version:apiVersion}/api/projects/{projectId:guid}")]
[Route("api/projects/{projectId:guid}")]
[Authorize]
public class FilesController : ControllerBase
{
    private readonly IMediator _mediator;

    public FilesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("files")]
    public async Task<IActionResult> GetProjectFiles(Guid projectId, CancellationToken ct)
    {
        var files = await _mediator.Send(new GetProjectFilesQuery(projectId), ct);
        return Ok(new { message = "Files retrieved successfully", data = files });
    }

    [HttpGet("files/{fileId:guid}/chunks")]
    public async Task<IActionResult> GetDocumentChunks(Guid projectId, Guid fileId, CancellationToken ct)
    {
        var chunks = await _mediator.Send(
            new GetDocumentChunksQuery(projectId, fileId), ct);
        return Ok(new { message = "Document chunks retrieved successfully", data = chunks });
    }

    public record GenerateFileUploadUrlRequest(string Filename, long FileSize, string FileType);

    [HttpPost("files/upload-url")]
    public async Task<IActionResult> GenerateFileUploadUrl(Guid projectId, GenerateFileUploadUrlRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(
            new Application.Files.Commands.GenerateFileUploadUrl.GenerateFileUploadUrlCommand(
                projectId, request.Filename, request.FileSize, request.FileType), ct);
        return Ok(new { message = "Upload URL generated successfully", upload_url = result.UploadUrl, s3_key = result.S3Key });
    }

    [HttpDelete("files/{fileId:guid}")]
    public async Task<IActionResult> DeleteProjectFile(Guid projectId, Guid fileId, CancellationToken ct)
    {
        await _mediator.Send(
            new Application.Files.Commands.DeleteProjectFile.DeleteProjectFileCommand(projectId, fileId), ct);
        return Ok(new { message = "File deleted successfully" });
    }

    public record ConfirmFileUploadRequest(string S3Key);

    [HttpPost("files/confirm-upload")]
    public async Task<IActionResult> ConfirmFileUpload(Guid projectId, ConfirmFileUploadRequest request, CancellationToken ct)
    {
        var document = await _mediator.Send(
            new Application.Files.Commands.ConfirmFileUpload.ConfirmFileUploadCommand(projectId, request.S3Key), ct);
        return Ok(new { message = "File upload confirmed successfully", data = document });
    }

    public record AddUrlDocumentRequest(string Url);

    [HttpPost("urls")]
    public async Task<IActionResult> AddUrlDocument(Guid projectId, AddUrlDocumentRequest request, CancellationToken ct)
    {
        var document = await _mediator.Send(
            new Application.Files.Commands.AddUrlDocument.AddUrlDocumentCommand(projectId, request.Url), ct);
        return Ok(new { message = "URL document added successfully", data = document });
    }
}
