namespace Application.Chats.Services.Models;

public record CitationDto(Guid ChunkId, Guid DocumentId, string Filename, int? Page);
