using Azure;
using Azure.AI.FormRecognizer.DocumentAnalysis;
using Backend.Models;
using Microsoft.Extensions.Logging;

namespace Backend.Services;

public class AzureDocumentIntelligenceExtractionService : IDocumentExtractionService
{
    private readonly DocumentAnalysisClient _client;
    private readonly ILogger<AzureDocumentIntelligenceExtractionService> _logger;

    public AzureDocumentIntelligenceExtractionService(
        DocumentAnalysisClient client,
        ILogger<AzureDocumentIntelligenceExtractionService> logger)
    {
        _client = client;
        _logger = logger;
    }

    public async Task<ExtractedDocumentData> ExtractAsync(Stream imageStream, string? fileName, CancellationToken ct = default)
    {
        _logger.LogInformation("AzureDocumentIntelligenceExtractionService: analyzing '{FileName}'", fileName);

        var operation = await _client.AnalyzeDocumentAsync(
            WaitUntil.Completed,
            "prebuilt-idDocument",
            imageStream,
            cancellationToken: ct);

        var result = operation.Value;
        var data = new ExtractedDocumentData();

        foreach (var doc in result.Documents)
        {
            if (doc.Fields.TryGetValue("FirstName", out var fn) && doc.Fields.TryGetValue("LastName", out var ln))
                data.Name = $"{fn.Content} {ln.Content}".Trim();
            else if (doc.Fields.TryGetValue("Name", out var name))
                data.Name = name.Content ?? string.Empty;

            if (doc.Fields.TryGetValue("Address", out var addr))
                data.Address = addr.Content ?? string.Empty;

            if (doc.Fields.TryGetValue("DateOfBirth", out var dob))
                data.DateOfBirth = dob.Content ?? string.Empty;

            if (doc.Fields.TryGetValue("DocumentNumber", out var docNum))
                data.LicenseNumber = docNum.Content;
        }

        _logger.LogInformation("AzureDocumentIntelligenceExtractionService: extracted Name={Name}", data.Name);
        return data;
    }
}
