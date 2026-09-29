using Application.Common.Interfaces;
using Application.Documents.Commands.ProcessDocument;
using MediatR;

namespace Infrastructure.Jobs;

public class DocumentProcessingJob : IDocumentProcessingJob
{
    private readonly IMediator _mediator;

    public DocumentProcessingJob(IMediator mediator)
    {
        _mediator = mediator;
    }

    public Task ProcessAsync(Guid documentId) => _mediator.Send(new ProcessDocumentCommand(documentId));
}
