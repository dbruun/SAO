using System.Text.RegularExpressions;
using Backend.Models;
using Microsoft.Extensions.Logging;

namespace Backend.Services;

public class StubAddressValidationService : IAddressValidationService
{
    private readonly ILogger<StubAddressValidationService> _logger;

    public StubAddressValidationService(ILogger<StubAddressValidationService> logger)
    {
        _logger = logger;
    }

    public Task<AddressValidationResult> ValidateAsync(string address, CancellationToken ct = default)
    {
        _logger.LogInformation("StubAddressValidationService: validating address");
        var messages = new List<string>();

        if (string.IsNullOrWhiteSpace(address))
        {
            messages.Add("Address is empty");
            return Task.FromResult(new AddressValidationResult { AddressValidated = false, Messages = messages });
        }

        bool hasNumber = Regex.IsMatch(address.Trim(), @"^\d+");
        if (!hasNumber)
            messages.Add("Address does not start with a street number");

        int wordCount = address.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length;
        if (wordCount < 3)
            messages.Add("Address appears too short");

        bool hasZip = Regex.IsMatch(address, @"\b\d{5}(-\d{4})?\b");
        if (!hasZip)
            messages.Add("No ZIP code detected (optional)");

        bool validated = hasNumber && wordCount >= 3;
        if (validated)
            messages.Add("Address passed plausibility check");

        _logger.LogInformation("StubAddressValidationService: validated={Validated}", validated);
        return Task.FromResult(new AddressValidationResult { AddressValidated = validated, Messages = messages });
    }
}
