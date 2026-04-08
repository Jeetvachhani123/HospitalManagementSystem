using System.Text.RegularExpressions;

namespace HospitalMS.BL.Validators;

public static class MedicalDataValidator
{
    // validate diagnosis text
    public static (bool IsValid, string? ErrorMessage) ValidateDiagnosis(string? diagnosis)
    {
        if (string.IsNullOrWhiteSpace(diagnosis))
            return (true, null);
       
        if (diagnosis.Length > 2000)
            return (false, "Diagnosis cannot exceed 2000 characters");
        
        if (ContainsDangerousPatterns(diagnosis))
            return (false, "Diagnosis contains potentially dangerous content");
        
        if (!ContainsValidMedicalContent(diagnosis))
            return (false, "Diagnosis should contain valid medical information");
       
        return (true, null);
    }

    // validate prescription text
    public static (bool IsValid, string? ErrorMessage) ValidatePrescription(string? prescription)
    {
        if (string.IsNullOrWhiteSpace(prescription))
            return (true, null);
       
        if (prescription.Length > 2000)
            return (false, "Prescription cannot exceed 2000 characters");
       
        if (ContainsDangerousPatterns(prescription))
            return (false, "Prescription contains potentially dangerous content");
       
        if (!ContainsValidPrescriptionFormat(prescription))
            return (false, "Prescription should follow proper medical format");
      
        return (true, null);
    }

    // validate allergies text
    public static (bool IsValid, string? ErrorMessage) ValidateAllergies(string? allergies)
    {
        if (string.IsNullOrWhiteSpace(allergies))
            return (true, null);
       
        if (allergies.Length > 1000)
            return (false, "Allergies cannot exceed 1000 characters");
       
        if (ContainsDangerousPatterns(allergies))
            return (false, "Allergies contain potentially dangerous content");
       
        if (!Regex.IsMatch(allergies, @"^[a-zA-Z0-9\s,;\-\(\)\.]+$"))
            return (false, "Allergies should only contain letters, numbers, and basic punctuation");
       
        return (true, null);
    }

    // validate medical history
    public static (bool IsValid, string? ErrorMessage) ValidateMedicalHistory(string? medicalHistory)
    {
        if (string.IsNullOrWhiteSpace(medicalHistory))
            return (true, null);
       
        if (medicalHistory.Length > 5000)
            return (false, "Medical history cannot exceed 5000 characters");
       
        if (ContainsDangerousPatterns(medicalHistory))
            return (false, "Medical history contains potentially dangerous content");
        
        return (true, null);
    }

    // validate blood group
    public static (bool IsValid, string? ErrorMessage) ValidateBloodGroup(string? bloodGroup)
    {
        
        if (string.IsNullOrWhiteSpace(bloodGroup))
            return (true, null);
       
        var validBloodGroups = new[] { "A+", "A-", "B+", "B-", "AB+", "AB-", "O+", "O-" };
        if (!validBloodGroups.Contains(bloodGroup.Trim(), StringComparer.OrdinalIgnoreCase))
            return (false, "Invalid blood group. Valid values: A+, A-, B+, B-, AB+, AB-, O+, O-");
       
        return (true, null);
    }

    // validate dosage format
    public static (bool IsValid, string? ErrorMessage) ValidateDosage(string? dosage)
    {
        if (string.IsNullOrWhiteSpace(dosage))
            return (true, null);
        
        if (!Regex.IsMatch(dosage, @"\d+\s*(mg|ml|g|mcg|tablet|capsule|drop|spray|puff|unit|iu|teaspoon|tablespoon)", RegexOptions.IgnoreCase))
            return (false, "Dosage should include amount and unit (e.g., '500mg', '2 tablets')");
        
        return (true, null);
    }

    // validate frequency format
    public static (bool IsValid, string? ErrorMessage) ValidateFrequency(string? frequency)
    {
        if (string.IsNullOrWhiteSpace(frequency))
            return (true, null);
        
        var validFrequencies = new[]
        {
            "once daily", "twice daily", "three times daily", "four times daily",
            "every 4 hours", "every 6 hours", "every 8 hours", "every 12 hours",
            "as needed", "before meals", "after meals", "at bedtime",
            "1x daily", "2x daily", "3x daily", "4x daily",
            "qd", "bid", "tid", "qid", "prn", "ac", "pc", "hs"
        };
        var normalized = frequency.ToLower().Trim();
        if (!validFrequencies.Any(f => normalized.Contains(f)))
            return (false, "Frequency should be a valid medical frequency (e.g., 'twice daily', 'every 8 hours')");
       
        return (true, null);
    }

    // check dangerous patterns
    private static bool ContainsDangerousPatterns(string text)
    {
        var dangerousPatterns = new[]
        {
            "<script", "javascript:", "onerror=", "onclick=", "onload=",
            "<iframe", "<object", "<embed",
            "eval(", "expression(",
            "vbscript:", "data:text/html"
        };
        return dangerousPatterns.Any(pattern => text.Contains(pattern, StringComparison.OrdinalIgnoreCase));
    }

    // check valid medical content
    private static bool ContainsValidMedicalContent(string text)
    {
        var hasLetters = Regex.IsMatch(text, @"[a-zA-Z]{3,}");
        var specialCharCount = text.Count(c => !char.IsLetterOrDigit(c) && !char.IsWhiteSpace(c));
        var notTooManySpecialChars = specialCharCount < text.Length * 0.3;
        return hasLetters && notTooManySpecialChars;
    }

    // check valid prescription format
    private static bool ContainsValidPrescriptionFormat(string text)
    {
        var hasLetters = Regex.IsMatch(text, @"[a-zA-Z]{3,}");
        var specialCharCount = text.Count(c => !char.IsLetterOrDigit(c) && !char.IsWhiteSpace(c));
        var notTooManySpecialChars = specialCharCount < text.Length * 0.3;
        return hasLetters && notTooManySpecialChars;
    }
}