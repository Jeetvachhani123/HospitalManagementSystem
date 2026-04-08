using FluentValidation;
using HospitalMS.BL.DTOs.Doctor;

namespace HospitalMS.BL.Validators;

// enhanced doctor create validator
public class DoctorCreateValidatorEnhanced : AbstractValidator<DoctorCreateDto>
{
    public DoctorCreateValidatorEnhanced()
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
      
        RuleFor(x => x.Specialization)
            .NotEmpty().WithMessage("Specialization is required")
            .MaximumLength(100).WithMessage("Specialization must not exceed 100 characters")
            .Must(BeSafeText).WithMessage("Specialization contains invalid characters");
       
        RuleFor(x => x.LicenseNumber)
            .NotEmpty().WithMessage("License number is required")
            .MaximumLength(50).WithMessage("License number must not exceed 50 characters")
            .Must(BeSafeLicenseNumber).WithMessage("License number contains invalid characters");
       
        RuleFor(x => x.Qualifications)
            .NotEmpty().WithMessage("Qualifications are required")
            .MaximumLength(500).WithMessage("Qualifications must not exceed 500 characters")
            .Must(BeSafeText).WithMessage("Qualifications contain invalid characters");
       
        RuleFor(x => x.Bio)
            .MaximumLength(1000).WithMessage("Bio must not exceed 1000 characters")
            .Must(BeSafeText).WithMessage("Bio contains invalid characters")
            .When(x => !string.IsNullOrWhiteSpace(x.Bio));
       
        RuleFor(x => x.YearsOfExperience)
            .GreaterThanOrEqualTo(0).WithMessage("Years of experience must be non-negative")
            .LessThanOrEqualTo(70).WithMessage("Years of experience seems unrealistic");
       
        RuleFor(x => x.ConsultationFee)
            .GreaterThan(0).WithMessage("Consultation fee must be greater than zero")
            .LessThanOrEqualTo(100000).WithMessage("Consultation fee seems unrealistic");
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
            return false;
        
        return System.Text.RegularExpressions.Regex.IsMatch(phoneNumber, @"^[\d\s\-\(\)\+]+$");
    }

    private bool BeSafeLicenseNumber(string? licenseNumber)
    {
        if (string.IsNullOrWhiteSpace(licenseNumber))
            return false;
        
        return System.Text.RegularExpressions.Regex.IsMatch(licenseNumber, @"^[a-zA-Z0-9\-/]+$");
    }

    private bool BeSafeText(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return true;
        
        var dangerousPatterns = new[] { "--", ";--", "/*", "*/", "xp_", "sp_", "<script", "javascript:" };
        return !dangerousPatterns.Any(pattern => text.Contains(pattern, StringComparison.OrdinalIgnoreCase));
    }
}