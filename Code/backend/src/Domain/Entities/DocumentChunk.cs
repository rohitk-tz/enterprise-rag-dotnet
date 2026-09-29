using SharedKernel;

namespace Domain.Entities;

public class DocumentChunk : Entity<Guid>
{
    public Guid DocumentId { get; private set; }
    public string Content { get; private set; }
    public int ChunkIndex { get; private set; }
    public int? PageNumber { get; private set; }
    public int CharCount { get; private set; }
    public string TypeJson { get; private set; }
    public string OriginalContentJson { get; private set; }
    public float[] Embedding { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }

    public DocumentChunk(
        Guid id, Guid documentId, string content, int chunkIndex, int? pageNumber,
        int charCount, string typeJson, string originalContentJson, float[] embedding,
        DateTimeOffset createdAt) : base(id)
    {
        DocumentId = documentId;
        Content = content;
        ChunkIndex = chunkIndex;
        PageNumber = pageNumber;
        CharCount = charCount;
        TypeJson = typeJson;
        OriginalContentJson = originalContentJson;
        Embedding = embedding;
        CreatedAt = createdAt;
    }
}
