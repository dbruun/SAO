using Backend.Models;

namespace Backend.Services;

public interface IBackendRecordsService
{
    /// <summary>Look up the authoritative record for a given driver's license number.</summary>
    Task<BackendRecord?> LookupByLicenseAsync(string licenseNumber, CancellationToken ct = default);
}
