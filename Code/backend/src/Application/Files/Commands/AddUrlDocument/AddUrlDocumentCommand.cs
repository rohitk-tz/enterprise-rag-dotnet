using Contracts.Files;
using MediatR;

namespace Application.Files.Commands.AddUrlDocument;

public record AddUrlDocumentCommand(Guid ProjectId, string Url) : IRequest<ProjectDocumentResponse>;
