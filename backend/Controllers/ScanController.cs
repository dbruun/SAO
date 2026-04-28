using Backend.Helpers;
using Backend.Models;
using Backend.Services;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ScanController : ControllerBase
{
    private readonly IDocumentScanService _scanService;
    private readonly ILogger<ScanController> _logger;

    public ScanController(IDocumentScanService scanService, ILogger<ScanController> logger)
    {
        _scanService = scanService;
        _logger      = logger;
    }

    /// <summary>
    /// Scan a document: extract data automatically and verify it against the backend records database.
    /// No manual data entry required — just upload the document.
    /// </summary>
    [HttpPost]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(VerificationResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<VerificationResult>> Scan(
        [FromForm] ScanRequest request,
        CancellationToken ct)
    {
        if (request.FileFront == null)
            return BadRequest(new { error = "fileFront is required" });

        _logger.LogInformation("ScanController: scan request received for file '{File}'",
            NormalizationHelper.SanitizeLog(request.FileFront.FileName));

        var result = await _scanService.ScanAsync(request, ct);
        return Ok(result);
    }
}
