using System.Globalization;

namespace EventHub.Localization;

public static class SupportedCultures
{
    public const string DefaultCulture = "en";

    public static readonly string[] Codes = ["en", "bg"];

    public static CultureInfo[] GetCultures()
        => Codes.Select(code => new CultureInfo(code)).ToArray();

    public static bool IsSupported(string? culture)
        => !string.IsNullOrWhiteSpace(culture)
            && Codes.Contains(culture.Trim(), StringComparer.OrdinalIgnoreCase);
}
