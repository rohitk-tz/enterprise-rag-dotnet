namespace Application.Chats.Services.Models;

public record ChunkContext(List<string> Texts, List<string> Images, List<string> Tables, List<CitationDto> Citations);
