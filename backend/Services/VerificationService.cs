using Backend.Helpers;
using Backend.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Backend.Services;

public class VerificationService : IVerificationService
{
    private readonly IDocumentExtractionService _extractor;
    private readonly IAddressValidationService _addressValidator;
    private readonly ILivenessService _livenessService;
    private readonly ILogger<VerificationService> _logger;
    private readonly double _nameThreshold;
    private readonly double _addressThreshold;

    public VerificationService(
        IDocumentExtractionService extractor,
        IAddressValidationService addressValidator,
        ILivenessService livenessService,
        IConfiguration config,
        ILogger<VerificationService> logger)
    {
        _extractor = extractor;
        _addressValidator = addressValidator;
        _livenessService = livenessService;
        _logger = logger;
        _nameThreshold = config.GetValue<double>("Verification:NameMatchThreshold", 0.90);
        _addressThreshold = config.GetValue<double>("Verification:AddressMatchThreshold", 0.88);
    }

    public async Task<VerificationResult> VerifyAsync(VerificationRequest request, CancellationToken ct = default)
    {
        _logger.LogInformation("VerificationService: starting pipeline");

        // Step 1: Extract document fields
        _logger.LogInformation("VerificationService: extraction start");
        using var frontStream = request.FileFront?.OpenReadStream() ?? Stream.Null;
        string? fileName = request.FileFront?.FileName;
        var extracted = await _extractor.ExtractAsync(frontStream, fileName, ct);
        _logger.LogInformation("VerificationService: extraction end — Name={Name}, Address={Address}, DOB={DOB}",
            extracted.Name, extracted.Address, extracted.DateOfBirth);

        // Step 2: Normalize values
        string normExtractedName = NormalizationHelper.NormalizeName(extracted.Name);
        string normUserName      = NormalizationHelper.NormalizeName(request.FullName);
        string normExtractedAddr = NormalizationHelper.NormalizeAddress(extracted.Address);
        string normUserAddr      = NormalizationHelper.NormalizeAddress(request.Address);
        string normExtractedDob  = NormalizationHelper.NormalizeDob(extracted.DateOfBirth);
        string normUserDob       = NormalizationHelper.NormalizeDob(request.DateOfBirth);
        _logger.LogInformation("VerificationService: normalized — ExtName={ExtName}, UserName={UserName}, ExtAddr={ExtAddr}, UserAddr={UserAddr}",
            normExtractedName, normUserName, normExtractedAddr, normUserAddr);

        // Step 3: Fuzzy match
        double nameSimilarity    = MatchingHelper.Similarity(normExtractedName, normUserName);
        double addressSimilarity = MatchingHelper.Similarity(normExtractedAddr, normUserAddr);
        bool dobMatch = string.Equals(normExtractedDob, normUserDob, StringComparison.OrdinalIgnoreCase);
        _logger.LogInformation("VerificationService: similarity — Name={NameSim:F3}, Address={AddrSim:F3}, DobMatch={DobMatch}",
            nameSimilarity, addressSimilarity, dobMatch);

        // Step 4: Address validation
        _logger.LogInformation("VerificationService: address validation start");
        var addrValidation = await _addressValidator.ValidateAsync(request.Address, ct);
        _logger.LogInformation("VerificationService: address validation end — Validated={Validated}", addrValidation.AddressValidated);

        // Step 5: Liveness check (stub)
        _logger.LogInformation("VerificationService: liveness check start");
        var liveness = await _livenessService.CheckAsync(null, ct);
        _logger.LogInformation("VerificationService: liveness check end — Status={Status}", liveness.Status);

        // Step 6: Score and decide
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
        _logger.LogInformation("VerificationService: final decision — Status={Status}, Confidence={Confidence}", status, confidence);

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
