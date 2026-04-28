namespace Backend.Helpers;

public class ScoringInput
{
    public double NameSimilarity { get; set; }
    public double AddressSimilarity { get; set; }
    public bool DobMatch { get; set; }
    public bool AddressValidated { get; set; }
    public double LivenessScore { get; set; } = 0.5;
    public double NameThreshold { get; set; }
    public double AddressThreshold { get; set; }
}

public static class ScoringHelper
{
    public static (double confidenceScore, string status, List<string> details) ComputeScore(ScoringInput input)
    {
        var details = new List<string>();

        // Weights: Name 37%, Address 37%, DOB 17%, AddrValidation 7%, Liveness 2%
        const double nameWeight = 0.37;
        const double addressWeight = 0.37;
        const double dobWeight = 0.17;
        const double addrValWeight = 0.07;
        const double livenessWeight = 0.02;

        double nameScore    = input.NameSimilarity  * 100.0 * nameWeight;
        double addressScore = input.AddressSimilarity * 100.0 * addressWeight;
        double dobScore     = (input.DobMatch ? 1.0 : 0.0) * 100.0 * dobWeight;
        double addrValScore = (input.AddressValidated ? 1.0 : 0.0) * 100.0 * addrValWeight;
        double livenessScore = input.LivenessScore * 100.0 * livenessWeight;

        double confidence = Math.Round(Math.Clamp(nameScore + addressScore + dobScore + addrValScore + livenessScore, 0, 100), 1);

        bool nameMatch    = input.NameSimilarity    >= input.NameThreshold;
        bool addressMatch = input.AddressSimilarity >= input.AddressThreshold;

        details.Add(nameMatch
            ? $"Name similarity {input.NameSimilarity:P1} — OK"
            : $"Name similarity {input.NameSimilarity:P1} below threshold {input.NameThreshold:P1}");

        details.Add(addressMatch
            ? $"Address similarity {input.AddressSimilarity:P1} — OK"
            : $"Address similarity {input.AddressSimilarity:P1} below threshold {input.AddressThreshold:P1}");

        details.Add(input.DobMatch ? "Date of birth matches" : "Date of birth does not match");

        details.Add(input.AddressValidated
            ? "Address passed plausibility check"
            : "Address failed plausibility check");

        details.Add("Liveness check not performed (stub)");

        string status;
        if (confidence >= 90 && nameMatch && addressMatch && input.DobMatch)
            status = "PASS";
        else if (confidence < 70 || !input.DobMatch || (input.NameSimilarity < 0.5 && input.AddressSimilarity < 0.5))
            status = "FAIL";
        else
            status = "REVIEW";

        return (confidence, status, details);
    }
}
