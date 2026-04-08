using FluentValidation;
using HospitalMS.BL.DTOs.Auth;
using HospitalMS.Models.Enums;

namespace HospitalMS.BL.Validators;

public class RegisterDtoValidator : AbstractValidator<RegisterDto>
{
    // define validation rules
    public RegisterDtoValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Invalid email format")
            .MaximumLength(100).WithMessage("Email cannot exceed 100 characters");
       
        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required")
            .MinimumLength(8).WithMessage("Password must be at least 8 characters")
            .MaximumLength(100).WithMessage("Password cannot exceed 100 characters")
            .Matches(@"[A-Z]").WithMessage("Password must contain at least one uppercase letter")
            .Matches(@"[a-z]").WithMessage("Password must contain at least one lowercase letter")
            .Matches(@"\d").WithMessage("Password must contain at least one number")
            .Matches(@"[\W_]").WithMessage("Password must contain at least one special character");
       
        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("First name is required")
            .MaximumLength(50).WithMessage("First name cannot exceed 50 characters")
            .Matches(@"^[a-zA-Z\s'-]+$").WithMessage("First name can only contain letters, spaces, hyphens, and apostrophes");
       
        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Last name is required")
            .MaximumLength(50).WithMessage("Last name cannot exceed 50 characters")
            .Matches(@"^[a-zA-Z\s'-]+$").WithMessage("Last name can only contain letters, spaces, hyphens, and apostrophes");
       
        RuleFor(x => x.PhoneNumber)
            .Matches(@"^\+?[1-9]\d{1,14}$").WithMessage("Invalid phone number format")
            .When(x => !string.IsNullOrWhiteSpace(x.PhoneNumber));
       
        RuleFor(x => x.Role)
            .IsInEnum().WithMessage("Invalid user role")
            .NotEqual(UserRole.Admin).WithMessage("Cannot register as Admin through this endpoint");
    }
}