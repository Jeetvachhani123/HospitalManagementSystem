using System.Text.RegularExpressions;
using System.Net;

namespace HospitalMS.Web.Helpers;

public static class InputSanitizer
{
    // sanitize html input
    public static string SanitizeHtml(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return string.Empty;
        return WebUtility.HtmlEncode(input);
    }

    // sanitize medical text
    public static string SanitizeMedicalText(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return string.Empty;
        var sanitized = Regex.Replace(input, @"<script[^>]*>.*?</script>", "", RegexOptions.IgnoreCase | RegexOptions.Singleline);
        sanitized = Regex.Replace(sanitized, @"on\w+\s*=\s*[""'][^""']*[""']", "", RegexOptions.IgnoreCase);
        sanitized = Regex.Replace(sanitized, @"javascript:", "", RegexOptions.IgnoreCase);
        sanitized = Regex.Replace(sanitized, @"<iframe[^>]*>.*?</iframe>", "", RegexOptions.IgnoreCase | RegexOptions.Singleline);
        sanitized = sanitized.Trim();
        if (sanitized.Length > 5000)
            sanitized = sanitized.Substring(0, 5000);
        return sanitized;
    }

    // sanitize allergy info
    public static string SanitizeAllergies(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return string.Empty;
        var sanitized = Regex.Replace(input, @"<[^>]+>", "");
        sanitized = Regex.Replace(sanitized, @"[^\w\s,;-]", "");
        sanitized = sanitized.Trim();
        if (sanitized.Length > 1000)
            sanitized = sanitized.Substring(0, 1000);
        return sanitized;
    }

    // sanitize general text
    public static string SanitizeText(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return string.Empty;
        var sanitized = WebUtility.HtmlEncode(input);
        sanitized = sanitized.Trim();
        return sanitized;
    }

    // sanitize phone number
    public static string SanitizePhoneNumber(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return string.Empty;
        var sanitized = Regex.Replace(input, @"[^\d\s\-\(\)\+]", "");
        return sanitized.Trim();
    }

    // sanitize email address
    public static string SanitizeEmail(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return string.Empty;
        var sanitized = input.Trim().ToLowerInvariant();
        if (!Regex.IsMatch(sanitized, @"^[a-z0-9._%+-]+@[a-z0-9.-]+\.[a-z]{2,}$"))
            return string.Empty;
        return sanitized;
    }

    // validate sql input
    public static string ValidateSqlInput(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return string.Empty;
        var dangerousPatterns = new[]
        {
            @"(\b(SELECT|INSERT|UPDATE|DELETE|DROP|CREATE|ALTER|EXEC|EXECUTE)\b)",
            @"(--|;|\/\*|\*\/)",
            @"('|('')|;|--|\/\*|\*\/|xp_|sp_)",
            @"(\bOR\b.*=.*)",
            @"(\bAND\b.*=.*)"
        };
        foreach (var pattern in dangerousPatterns)
        {
            if (Regex.IsMatch(input, pattern, RegexOptions.IgnoreCase))
            {
                throw new ArgumentException("Input contains potentially dangerous SQL patterns");
            }
        }
        return input.Trim();
    }
}