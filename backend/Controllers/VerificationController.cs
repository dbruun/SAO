using Backend.Helpers;
using Backend.Models;
using Backend.Services;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class VerificationController : ControllerBase
{
    private readonly IVerificationService _verificationService;
    private readonly ILogger<VerificationController> _logger;

    public VerificationController(
        IVerificationService verificationService,
        ILogger<VerificationController> logger)
    {
        _verificationService = verificationService;
        _logger = logger;
    }

    /// <summary>Verify identity from a driver's license image against user-provided data.</summary>
    [HttpPost]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(VerificationResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<VerificationResult>> Verify(
        [FromForm] VerificationRequest request,
        CancellationToken ct)
    {
        if (request.FileFront == null)
            return BadRequest(new { error = "fileFront is required" });
        if (string.IsNullOrWhiteSpace(request.FullName))
            return BadRequest(new { error = "fullName is required" });
        if (string.IsNullOrWhiteSpace(request.Address))
            return BadRequest(new { error = "address is required" });
        if (string.IsNullOrWhiteSpace(request.DateOfBirth))
            return BadRequest(new { error = "dateOfBirth is required" });

        _logger.LogInformation("VerificationController: request received for name={Name}",
            NormalizationHelper.SanitizeLog(request.FullName));

        var result = await _verificationService.VerifyAsync(request, ct);
        return Ok(result);
    }
}
