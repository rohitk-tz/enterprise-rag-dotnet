using MediatR;

namespace Application.Documents.Commands.ProcessDocument;

public record ProcessDocumentCommand(Guid DocumentId) : IRequest;
