using System.Text.RegularExpressions;

namespace Backend.Helpers;

public static class NormalizationHelper
{
    private static readonly Dictionary<string, string> AbbreviationMap = new(StringComparer.OrdinalIgnoreCase)
    {
        { "St", "Street" }, { "Rd", "Road" }, { "Ave", "Avenue" }, { "Blvd", "Boulevard" },
        { "Dr", "Drive" }, { "Ln", "Lane" }, { "Ct", "Court" }, { "Pl", "Place" },
        { "Apt", "Apartment" }, { "Ste", "Suite" },
        { "N", "North" }, { "S", "South" }, { "E", "East" }, { "W", "West" }
    };

    public static string NormalizeName(string name)
    {
        if (string.IsNullOrWhiteSpace(name)) return string.Empty;
        name = Regex.Replace(name, @"[^\w\s]", " ");
        name = Regex.Replace(name, @"\s+", " ").Trim();
        return name.ToUpperInvariant();
    }

    public static string NormalizeAddress(string address)
    {
        if (string.IsNullOrWhiteSpace(address)) return string.Empty;

        address = Regex.Replace(address, @"[,\.#]", " ");

        var tokens = address.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
        var expanded = tokens.Select(t =>
            AbbreviationMap.TryGetValue(t, out var full) ? full : t);

        address = string.Join(" ", expanded);
        address = Regex.Replace(address, @"\s+", " ").Trim();
        return address.ToUpperInvariant();
    }

    public static string NormalizeDob(string dob)
    {
        if (string.IsNullOrWhiteSpace(dob)) return string.Empty;
        if (DateTime.TryParse(dob, out var dt))
            return dt.ToString("yyyy-MM-dd");
        return dob.Trim();
    }

    public static string SanitizeLog(string? value)
    {
        if (string.IsNullOrEmpty(value)) return string.Empty;
        return value.Replace("\r", " ").Replace("\n", " ").Replace("\t", " ");
    }
}
