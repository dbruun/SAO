namespace Backend.Models;

public class VerificationChecks
{
    public bool NameMatch { get; set; }
    public bool AddressMatch { get; set; }
    public bool DobMatch { get; set; }
    public bool AddressValidated { get; set; }
    public string Liveness { get; set; } = "NotPerformed";
}

public class SimilarityScores
{
    public double NameScore { get; set; }
    public double AddressScore { get; set; }
}

public class VerificationResult
{
    public string Status { get; set; } = string.Empty;
    public double ConfidenceScore { get; set; }
    public VerificationChecks Checks { get; set; } = new();
    public SimilarityScores Similarity { get; set; } = new();
    public List<string> Details { get; set; } = new();
    public ExtractedDocumentData Extracted { get; set; } = new();
}
