using Backend.Models;

namespace Backend.Services;

public interface IDocumentScanService
{
    /// <summary>Extract data from a document and verify it against the backend records database.</summary>
    Task<VerificationResult> ScanAsync(ScanRequest request, CancellationToken ct = default);
}
