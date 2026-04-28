using Backend.Models;

namespace Backend.Services;

public interface IAddressValidationService
{
    Task<AddressValidationResult> ValidateAsync(string address, CancellationToken ct = default);
}
