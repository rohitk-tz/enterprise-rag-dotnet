namespace Application.Common.Interfaces;

/// <summary>
/// Thin Hangfire-invocable wrapper around document processing, defined here (in Application)
/// so that command handlers can enqueue it by type without Application taking a project
/// reference on Infrastructure (which would create a circular reference, since Infrastructure
/// already references Application). Infrastructure provides the concrete implementation.
/// </summary>
public interface IDocumentProcessingJob
{
    Task ProcessAsync(Guid documentId);
}
