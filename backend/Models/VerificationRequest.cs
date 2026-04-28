namespace Backend.Models;

public class VerificationRequest
{
    public IFormFile? FileFront { get; set; }
    public IFormFile? FileBack { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string DateOfBirth { get; set; } = string.Empty;
}
