namespace Application.Common.Models;

public record RawChunk(
    string Text,
    IReadOnlyList<string> Tables,
    IReadOnlyList<string> Images,
    IReadOnlyList<string> Types,
    int? PageNumber,
    int CharCount);
