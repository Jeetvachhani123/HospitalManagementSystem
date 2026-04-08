using FluentValidation;
using HospitalMS.BL.DTOs.Auth;

namespace HospitalMS.BL.Validators;

public class LoginDtoValidator : AbstractValidator<LoginDto>
{
    public LoginDtoValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Invalid email format")
            .MaximumLength(100).WithMessage("Email cannot exceed 100 characters");
       
        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required")
            .MinimumLength(1).WithMessage("Password is required");
    }
}