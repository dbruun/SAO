using Backend.Models;
using Microsoft.Extensions.Logging;

namespace Backend.Services;

/// <summary>
/// In-memory mock of the backend records database.
/// In a real system this would call a database or authoritative records API.
/// </summary>
public class MockBackendRecordsService : IBackendRecordsService
{
    private readonly ILogger<MockBackendRecordsService> _logger;

    // Authoritative records keyed by driver's license number
    private static readonly Dictionary<string, BackendRecord> Records = new(StringComparer.OrdinalIgnoreCase)
    {
        ["S123-4567-8901"] = new BackendRecord
        {
            FullName    = "John Michael Smith",
            Address     = "123 Main Street, Springfield, IL 62701",
            DateOfBirth = "1985-06-15"
        }
    };

    public MockBackendRecordsService(ILogger<MockBackendRecordsService> logger)
    {
        _logger = logger;
    }

    public Task<BackendRecord?> LookupByLicenseAsync(string licenseNumber, CancellationToken ct = default)
    {
        _logger.LogInformation("MockBackendRecordsService: looking up license '{License}'", licenseNumber);
        Records.TryGetValue(licenseNumber ?? string.Empty, out var record);
        _logger.LogInformation("MockBackendRecordsService: lookup result — {Result}", record != null ? "found" : "not found");
        return Task.FromResult(record);
    }
}
