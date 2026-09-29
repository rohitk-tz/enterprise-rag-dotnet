using Contracts.Files;
using MediatR;

namespace Application.Files.Queries.GetProjectFiles;

public record GetProjectFilesQuery(Guid ProjectId) : IRequest<List<ProjectDocumentResponse>>;
