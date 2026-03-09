using FluentValidation;
using HospitalMS.BL.DTOs.Patient;

namespace HospitalMS.BL.Validators;

public class PatientCreateValidator : AbstractValidator<PatientCreateDto>
{
    public PatientCreateValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Invalid email format");
        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required")
            .MinimumLength(6).WithMessage("Password must be at least 6 characters");
        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("First Name is required")
            .MaximumLength(50).WithMessage("First Name cannot exceed 50 characters");
        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Last Name is required")
            .MaximumLength(50).WithMessage("Last Name cannot exceed 50 characters");
        RuleFor(x => x.DateOfBirth)
            .NotEmpty().WithMessage("Date of Birth is required")
            .LessThan(DateTime.Today).WithMessage("Date of Birth must be in the past");
        RuleFor(x => x.PhoneNumber)
            .Matches(@"^\+?[1-9]\d{1,14}$").When(x => !string.IsNullOrEmpty(x.PhoneNumber))
            .WithMessage("Invalid phone number format");
        RuleFor(x => x.BloodGroup)
            .Must(BeValidBloodGroup).When(x => !string.IsNullOrEmpty(x.BloodGroup))
            .WithMessage("Invalid Blood Group (A+, A-, B+, B-, AB+, AB-, O+, O-)");
    }

    // check valid blood group
    private bool BeValidBloodGroup(string? bloodGroup)
    {
        var validGroups = new[] { "A+", "A-", "B+", "B-", "AB+", "AB-", "O+", "O-" };
        return validGroups.Contains(bloodGroup);
    }
}