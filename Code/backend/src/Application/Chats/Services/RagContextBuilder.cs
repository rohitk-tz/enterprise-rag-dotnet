using System.Text.Json;
using Application.Chats.Services.Models;
using Domain.Entities;

namespace Application.Chats.Services;

public static class RagContextBuilder
{
    private record OriginalContent(string? Text, List<string>? Tables, List<string>? Images);

    // The original_content JSON stored by the Python pipeline uses lowercase keys
    // (text/tables/images). System.Text.Json's default deserialization is
    // case-sensitive, so PropertyNameCaseInsensitive is required here or every
    // chunk would deserialize to all-null content.
    private static readonly JsonSerializerOptions DeserializeOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public static ChunkContext Build(List<DocumentChunk> chunks, Dictionary<Guid, string> filenames)
    {
        var texts = new List<string>();
        var images = new List<string>();
        var tables = new List<string>();
        var citations = new List<CitationDto>();

        foreach (var chunk in chunks)
        {
            var original = JsonSerializer.Deserialize<OriginalContent>(chunk.OriginalContentJson, DeserializeOptions)
                ?? new OriginalContent(null, null, null);

            if (!string.IsNullOrEmpty(original.Text))
            {
                texts.Add(original.Text);
            }
            if (original.Images is { Count: > 0 })
            {
                images.AddRange(original.Images);
            }
            if (original.Tables is { Count: > 0 })
            {
                tables.AddRange(original.Tables);
            }

            // Every chunk gets a citation unconditionally, matching
            // server/routes/chats.py:build_context exactly (see docs/migration/08-RAG-Architecture.md).
            var filename = filenames.GetValueOrDefault(chunk.DocumentId, "Unknown Document");
            citations.Add(new CitationDto(chunk.Id, chunk.DocumentId, filename, chunk.PageNumber));
        }

        return new ChunkContext(texts, images, tables, citations);
    }
}
