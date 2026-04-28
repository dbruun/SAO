namespace Backend.Models;

public class ScanRequest
{
    public IFormFile? FileFront { get; set; }
    public IFormFile? FileBack { get; set; }
}
