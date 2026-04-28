using Backend.Helpers;
using Backend.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Backend.Services;

public class DocumentScanService : IDocumentScanService
{
    private readonly IDocumentExtractionService _extractor;
    private readonly IBackendRecordsService _records;
    private readonly IAddressValidationService _addressValidator;
    private readonly ILivenessService _livenessService;
    private readonly ILogger<DocumentScanService> _logger;
    private readonly double _nameThreshold;
    private readonly double _addressThreshold;

    public DocumentScanService(
        IDocumentExtractionService extractor,
        IBackendRecordsService records,
        IAddressValidationService addressValidator,
        ILivenessService livenessService,
        IConfiguration config,
        ILogger<DocumentScanService> logger)
    {
        _extractor        = extractor;
        _records          = records;
        _addressValidator = addressValidator;
        _livenessService  = livenessService;
        _logger           = logger;
        _nameThreshold    = config.GetValue<double>("Verification:NameMatchThreshold", 0.90);
        _addressThreshold = config.GetValue<double>("Verification:AddressMatchThreshold", 0.88);
    }

    public async Task<VerificationResult> ScanAsync(ScanRequest request, CancellationToken ct = default)
    {
        _logger.LogInformation("DocumentScanService: starting scan pipeline");

        // Step 1: Extract document fields (FileFront is validated non-null by the controller)
        using var frontStream = request.FileFront!.OpenReadStream();
        string? fileName = request.FileFront.FileName;
        var extracted = await _extractor.ExtractAsync(frontStream, fileName, ct);
        _logger.LogInformation("DocumentScanService: extracted — Name={Name}, License={License}",
            extracted.Name, extracted.LicenseNumber);

        // Step 2: Look up authoritative backend record by license number
        var record = await _records.LookupByLicenseAsync(extracted.LicenseNumber ?? string.Empty, ct);

        if (record == null)
        {
            _logger.LogInformation("DocumentScanService: no backend record found — returning FAIL");
            return new VerificationResult
            {
                Status          = "FAIL",
                ConfidenceScore = 0,
                Checks = new VerificationChecks
                {
                    NameMatch        = false,
                    AddressMatch     = false,
                    DobMatch         = false,
                    AddressValidated = false,
                    Liveness         = "NotPerformed"
                },
                Similarity = new SimilarityScores { NameScore = 0, AddressScore = 0 },
                Details    = new List<string> { "No backend record found for this document's license number." },
                Extracted  = extracted
            };
        }

        // Step 3: Normalize values
        string normExtractedName = NormalizationHelper.NormalizeName(extracted.Name);
        string normRecordName    = NormalizationHelper.NormalizeName(record.FullName);
        string normExtractedAddr = NormalizationHelper.NormalizeAddress(extracted.Address);
        string normRecordAddr    = NormalizationHelper.NormalizeAddress(record.Address);
        string normExtractedDob  = NormalizationHelper.NormalizeDob(extracted.DateOfBirth);
        string normRecordDob     = NormalizationHelper.NormalizeDob(record.DateOfBirth);

        // Step 4: Fuzzy match document data against backend record
        double nameSimilarity    = MatchingHelper.Similarity(normExtractedName, normRecordName);
        double addressSimilarity = MatchingHelper.Similarity(normExtractedAddr, normRecordAddr);
        bool dobMatch = string.Equals(normExtractedDob, normRecordDob, StringComparison.OrdinalIgnoreCase);
        _logger.LogInformation("DocumentScanService: similarity — Name={NameSim:F3}, Address={AddrSim:F3}, DobMatch={DobMatch}",
            nameSimilarity, addressSimilarity, dobMatch);

        // Step 5: Address validation
        var addrValidation = await _addressValidator.ValidateAsync(extracted.Address, ct);

        // Step 6: Liveness check (stub)
        var liveness = await _livenessService.CheckAsync(null, ct);

        // Step 7: Score and decide
        var (confidence, status, details) = ScoringHelper.ComputeScore(new ScoringInput
        {
            NameSimilarity    = nameSimilarity,
            AddressSimilarity = addressSimilarity,
            DobMatch          = dobMatch,
            AddressValidated  = addrValidation.AddressValidated,
            LivenessScore     = liveness.Score,
            NameThreshold     = _nameThreshold,
            AddressThreshold  = _addressThreshold
        });

        _logger.LogInformation("DocumentScanService: final decision — Status={Status}, Confidence={Confidence}", status, confidence);

        return new VerificationResult
        {
            Status          = status,
            ConfidenceScore = confidence,
            Checks = new VerificationChecks
            {
                NameMatch        = nameSimilarity    >= _nameThreshold,
                AddressMatch     = addressSimilarity >= _addressThreshold,
                DobMatch         = dobMatch,
                AddressValidated = addrValidation.AddressValidated,
                Liveness         = liveness.Status.ToString()
            },
            Similarity = new SimilarityScores
            {
                NameScore    = Math.Round(nameSimilarity, 3),
                AddressScore = Math.Round(addressSimilarity, 3)
            },
            Details   = details,
            Extracted = extracted
        };
    }
}
