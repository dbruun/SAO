namespace Backend.Models;

public class ExtractedDocumentData
{
    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string DateOfBirth { get; set; } = string.Empty;
    public string? LicenseNumber { get; set; }
}
