using FluentValidation;
using HospitalMS.BL.DTOs.Doctor;
using System.Text.RegularExpressions;

namespace HospitalMS.BL.Validators;

public class DoctorUpdateValidator : AbstractValidator<DoctorUpdateDto>
{
    public DoctorUpdateValidator()
    {
        RuleFor(x => x.PhoneNumber)
            .MaximumLength(20).WithMessage("Phone number must not exceed 20 characters")
            .Must(BeSafePhoneNumber).WithMessage("Phone number contains invalid characters")
            .When(x => !string.IsNullOrWhiteSpace(x.PhoneNumber));
       
        RuleFor(x => x.Specialization)
            .MaximumLength(100).WithMessage("Specialization must not exceed 100 characters")
            .Must(BeSafeText).WithMessage("Specialization contains invalid characters")
            .When(x => !string.IsNullOrWhiteSpace(x.Specialization));
      
        RuleFor(x => x.YearsOfExperience)
            .GreaterThanOrEqualTo(0).WithMessage("Years of experience must be non-negative")
            .LessThanOrEqualTo(70).WithMessage("Years of experience seems unrealistic")
            .When(x => x.YearsOfExperience.HasValue);
     
        RuleFor(x => x.Qualifications)
            .MaximumLength(500).WithMessage("Qualifications must not exceed 500 characters")
            .Must(BeSafeText).WithMessage("Qualifications contain invalid characters")
            .When(x => !string.IsNullOrWhiteSpace(x.Qualifications));
       
        RuleFor(x => x.Bio)
            .MaximumLength(1000).WithMessage("Bio must not exceed 1000 characters")
            .Must(BeSafeText).WithMessage("Bio contains invalid characters")
            .When(x => !string.IsNullOrWhiteSpace(x.Bio));
        
        RuleFor(x => x.ConsultationFee)
            .GreaterThan(0).WithMessage("Consultation fee must be greater than zero")
            .LessThanOrEqualTo(100000).WithMessage("Consultation fee seems unrealistic")
            .When(x => x.ConsultationFee.HasValue);
    }

    // check safe phone number
    private bool BeSafePhoneNumber(string? phoneNumber)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber))
            return true;
        
        return Regex.IsMatch(phoneNumber, @"^[\d\s\-\(\)\+]+$");
    }

    // check safe text
    private bool BeSafeText(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return true;
       
        var dangerousPatterns = new[] { "<script", "javascript:", "onerror=", "onclick=", "--", ";--", "/*", "*/" };
        return !dangerousPatterns.Any(pattern => text.Contains(pattern, StringComparison.OrdinalIgnoreCase));
    }
}