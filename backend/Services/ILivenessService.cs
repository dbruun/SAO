using Backend.Models;

namespace Backend.Services;

public interface ILivenessService
{
    Task<LivenessResult> CheckAsync(Stream? imageStream, CancellationToken ct = default);
}
