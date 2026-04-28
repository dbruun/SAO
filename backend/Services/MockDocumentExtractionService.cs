using Backend.Models;
using Microsoft.Extensions.Logging;

namespace Backend.Services;

/// <summary>
/// Returns deterministic mock data based on the uploaded filename.
/// Profiles:
///   filename contains "pass"   → perfect match (PASS expected)
///   filename contains "review" → address has extra apartment info (REVIEW expected)
///   anything else              → completely different person/DOB (FAIL expected)
/// </summary>
public class MockDocumentExtractionService : IDocumentExtractionService
{
    private readonly ILogger<MockDocumentExtractionService> _logger;

    // PASS profile: exact match with demo user input
    private static readonly ExtractedDocumentData PassProfile = new()
    {
        Name = "John Michael Smith",
        Address = "123 Main Street, Springfield, IL 62701",
        DateOfBirth = "1985-06-15",
        LicenseNumber = "S123-4567-8901"
    };

    // REVIEW profile: address has extra unit info → moderate similarity, address check fails
    private static readonly ExtractedDocumentData ReviewProfile = new()
    {
        Name = "John Michael Smith",
        Address = "123 Main Street Apt 2B, Springfield, IL 62701",
        DateOfBirth = "1985-06-15",
        LicenseNumber = "S123-4567-8901"
    };

    // FAIL profile: different person + DOB mismatch
    private static readonly ExtractedDocumentData FailProfile = new()
    {
        Name = "Jane Doe",
        Address = "999 Oak Avenue, Chicago, IL 60601",
        DateOfBirth = "1990-01-01",
        LicenseNumber = "D999-0000-1111"
    };

    public MockDocumentExtractionService(ILogger<MockDocumentExtractionService> logger)
    {
        _logger = logger;
    }

    public Task<ExtractedDocumentData> ExtractAsync(Stream imageStream, string? fileName, CancellationToken ct = default)
    {
        _logger.LogInformation("MockDocumentExtractionService: extracting from file '{FileName}'", fileName);

        var lower = (fileName ?? string.Empty).ToLowerInvariant();
        ExtractedDocumentData result = lower.Contains("pass") ? PassProfile
                                     : lower.Contains("review") ? ReviewProfile
                                     : FailProfile;

        _logger.LogInformation("MockDocumentExtractionService: extracted Name={Name}, DOB={DOB}", result.Name, result.DateOfBirth);
        return Task.FromResult(result);
    }
}
