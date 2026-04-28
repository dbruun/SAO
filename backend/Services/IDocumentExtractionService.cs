using Backend.Models;

namespace Backend.Services;

public interface IDocumentExtractionService
{
    Task<ExtractedDocumentData> ExtractAsync(Stream imageStream, string? fileName, CancellationToken ct = default);
}
