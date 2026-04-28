using Backend.Models;

namespace Backend.Services;

public interface IVerificationService
{
    Task<VerificationResult> VerifyAsync(VerificationRequest request, CancellationToken ct = default);
}
