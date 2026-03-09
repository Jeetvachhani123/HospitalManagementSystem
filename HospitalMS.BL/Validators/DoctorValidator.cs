using FluentValidation;
using HospitalMS.BL.DTOs.Doctor;

namespace HospitalMS.BL.Validators;

public class DoctorCreateValidator : AbstractValidator<DoctorCreateDto>
{
    public DoctorCreateValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Invalid email format");
        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required")
            .MinimumLength(6).WithMessage("Password must be at least 6 characters");
        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("First name is required")
            .MaximumLength(100).WithMessage("First name cannot exceed 100 characters");
        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Last name is required")
            .MaximumLength(100).WithMessage("Last name cannot exceed 100 characters");
        RuleFor(x => x.Specialization)
            .NotEmpty().WithMessage("Specialization is required")
            .MaximumLength(200).WithMessage("Specialization cannot exceed 200 characters");
        RuleFor(x => x.LicenseNumber)
            .NotEmpty().WithMessage("License number is required")
            .MaximumLength(100).WithMessage("License number cannot exceed 100 characters");
        RuleFor(x => x.YearsOfExperience)
            .GreaterThanOrEqualTo(0).WithMessage("Years of experience cannot be negative");
        RuleFor(x => x.ConsultationFee)
            .GreaterThanOrEqualTo(0).WithMessage("Consultation fee cannot be negative");
    }
}