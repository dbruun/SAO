namespace Backend.Models;

public class AddressValidationResult
{
    public bool AddressValidated { get; set; }
    public List<string> Messages { get; set; } = new();
}
