using Contracts.Files;
using MediatR;

namespace Application.Files.Queries.GetDocumentChunks;

public record GetDocumentChunksQuery(Guid ProjectId, Guid FileId) : IRequest<List<DocumentChunkResponse>>;
