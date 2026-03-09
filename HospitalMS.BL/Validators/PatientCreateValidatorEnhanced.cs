using FluentValidation;
using HospitalMS.BL.DTOs.Patient;

namespace HospitalMS.BL.Validators;

public class PatientCreateValidatorEnhanced : AbstractValidator<PatientCreateDto>
{
    public PatientCreateValidatorEnhanced()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Invalid email format")
            .MaximumLength(100).WithMessage("Email must not exceed 100 characters")
            .Must(BeValidEmail).WithMessage("Email contains invalid characters");
        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required")
            .MinimumLength(8).WithMessage("Password must be at least 8 characters")
            .MaximumLength(100).WithMessage("Password must not exceed 100 characters")
            .Matches(@"[A-Z]").WithMessage("Password must contain at least one uppercase letter")
            .Matches(@"[a-z]").WithMessage("Password must contain at least one lowercase letter")
            .Matches(@"[0-9]").WithMessage("Password must contain at least one number")
            .Matches(@"[\W_]").WithMessage("Password must contain at least one special character");
        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("First name is required")
            .MaximumLength(50).WithMessage("First name must not exceed 50 characters")
            .Must(BeSafeName).WithMessage("First name contains invalid characters");
        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Last name is required")
            .MaximumLength(50).WithMessage("Last name must not exceed 50 characters")
            .Must(BeSafeName).WithMessage("Last name contains invalid characters");
        RuleFor(x => x.PhoneNumber)
            .NotEmpty().WithMessage("Phone number is required")
            .MaximumLength(20).WithMessage("Phone number must not exceed 20 characters")
            .Must(BeSafePhoneNumber).WithMessage("Phone number contains invalid characters");
        RuleFor(x => x.EmergencyContact)
            .MaximumLength(20).WithMessage("Emergency contact must not exceed 20 characters")
            .Must(BeSafePhoneNumber).WithMessage("Emergency contact contains invalid characters")
            .When(x => !string.IsNullOrWhiteSpace(x.EmergencyContact));
        RuleFor(x => x.DateOfBirth)
            .NotEmpty().WithMessage("Date of birth is required")
            .LessThan(DateTime.Today).WithMessage("Date of birth must be in the past")
            .GreaterThan(DateTime.Today.AddYears(-150)).WithMessage("Date of birth seems unrealistic (more than 150 years ago)")
            .Must(BeValidAge).WithMessage("Patient must be at least born (age 0-150 years)");
    }

    private bool BeValidAge(DateTime dateOfBirth)
    {
        var age = DateTime.Today.Year - dateOfBirth.Year;
        if (dateOfBirth.Date > DateTime.Today.AddYears(-age))
            age--;
        return age >= 0 && age <= 150;
    }

    private bool BeValidEmail(string? email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return false;
        var dangerousPatterns = new[] { "--", ";", "/*", "*/", "xp_", "sp_", "'" };
        return !dangerousPatterns.Any(pattern => email.Contains(pattern, StringComparison.OrdinalIgnoreCase));
    }

    private bool BeSafeName(string? name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return false;
        return System.Text.RegularExpressions.Regex.IsMatch(name, @"^[a-zA-Z\s\-']+$");
    }

    private bool BeSafePhoneNumber(string? phoneNumber)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber))
            return true;
        return System.Text.RegularExpressions.Regex.IsMatch(phoneNumber, @"^[\d\s\-\(\)\+]+$");
    }
}