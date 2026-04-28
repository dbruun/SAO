using Backend.Models;
using Microsoft.Extensions.Logging;

namespace Backend.Services;

public class StubLivenessService : ILivenessService
{
    private readonly ILogger<StubLivenessService> _logger;

    public StubLivenessService(ILogger<StubLivenessService> logger)
    {
        _logger = logger;
    }

    public Task<LivenessResult> CheckAsync(Stream? imageStream, CancellationToken ct = default)
    {
        _logger.LogInformation("StubLivenessService: returning NotPerformed");
        return Task.FromResult(new LivenessResult
        {
            Status = LivenessStatus.NotPerformed,
            Score = 0.5
        });
    }
}
