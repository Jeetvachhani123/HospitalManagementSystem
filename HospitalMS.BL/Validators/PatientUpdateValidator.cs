using FluentValidation;
using HospitalMS.BL.DTOs.Patient;
using System.Text.RegularExpressions;

namespace HospitalMS.BL.Validators;

public class PatientUpdateValidator : AbstractValidator<PatientUpdateDto>
{
    public PatientUpdateValidator()
    {
        RuleFor(x => x.PhoneNumber)
            .MaximumLength(20).WithMessage("Phone number must not exceed 20 characters")
            .Must(BeSafePhoneNumber).WithMessage("Phone number contains invalid characters")
            .When(x => !string.IsNullOrWhiteSpace(x.PhoneNumber));
        RuleFor(x => x.BloodGroup)
            .Must(BeValidBloodGroup).WithMessage("Invalid blood group. Valid values: A+, A-, B+, B-, AB+, AB-, O+, O-")
            .When(x => !string.IsNullOrWhiteSpace(x.BloodGroup));
        RuleFor(x => x.Gender)
            .Must(BeValidGender).WithMessage("Invalid gender. Valid values: Male, Female, Other")
            .When(x => !string.IsNullOrWhiteSpace(x.Gender));
        RuleFor(x => x.EmergencyContact)
            .MaximumLength(20).WithMessage("Emergency contact must not exceed 20 characters")
            .Must(BeSafePhoneNumber).WithMessage("Emergency contact contains invalid characters")
            .When(x => !string.IsNullOrWhiteSpace(x.EmergencyContact));
        RuleFor(x => x.MedicalHistory)
            .MaximumLength(5000).WithMessage("Medical history must not exceed 5000 characters")
            .Must(BeSafeMedicalText).WithMessage("Medical history contains potentially dangerous content")
            .When(x => !string.IsNullOrWhiteSpace(x.MedicalHistory));
        RuleFor(x => x.Allergies)
            .MaximumLength(1000).WithMessage("Allergies must not exceed 1000 characters")
            .Must(BeSafeAllergies).WithMessage("Allergies contain invalid characters")
            .When(x => !string.IsNullOrWhiteSpace(x.Allergies));
    }

    // check safe phone number
    private bool BeSafePhoneNumber(string? phoneNumber)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber))
            return true;
        return Regex.IsMatch(phoneNumber, @"^[\d\s\-\(\)\+]+$");
    }

    // check valid blood group
    private bool BeValidBloodGroup(string? bloodGroup)
    {
        if (string.IsNullOrWhiteSpace(bloodGroup))
            return true;
        var validBloodGroups = new[] { "A+", "A-", "B+", "B-", "AB+", "AB-", "O+", "O-" };
        return validBloodGroups.Contains(bloodGroup.Trim(), StringComparer.OrdinalIgnoreCase);
    }

    // check valid gender
    private bool BeValidGender(string? gender)
    {
        if (string.IsNullOrWhiteSpace(gender))
            return true;
        var validGenders = new[] { "Male", "Female", "Other" };
        return validGenders.Contains(gender.Trim(), StringComparer.OrdinalIgnoreCase);
    }

    // check safe medical text
    private bool BeSafeMedicalText(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return true;
        var dangerousPatterns = new[] { "<script", "javascript:", "onerror=", "onclick=", "--", ";--", "/*", "*/" };
        return !dangerousPatterns.Any(pattern => text.Contains(pattern, StringComparison.OrdinalIgnoreCase));
    }

    // check safe allergies
    private bool BeSafeAllergies(string? allergies)
    {
        if (string.IsNullOrWhiteSpace(allergies))
            return true;
        return Regex.IsMatch(allergies, @"^[a-zA-Z0-9\s,;\-\(\)\.]+$");
    }
}