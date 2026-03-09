using FluentValidation;
using HospitalMS.BL.DTOs.Appointment;

namespace HospitalMS.BL.Validators;

public class AppointmentCreateValidator : AbstractValidator<AppointmentCreateDto>
{
    public AppointmentCreateValidator()
    {
        RuleFor(x => x.PatientId)
            .GreaterThan(0).WithMessage("Patient ID is required");
        RuleFor(x => x.DoctorId)
            .GreaterThan(0).WithMessage("Doctor ID is required");
        RuleFor(x => x.AppointmentDate)
            .NotEmpty().WithMessage("Appointment date is required")
            .GreaterThanOrEqualTo(DateTime.Today).WithMessage("Appointment date must be today or in the future");
        RuleFor(x => x.StartTime)
            .NotEmpty().WithMessage("Start time is required");
        RuleFor(x => x.EndTime)
            .NotEmpty().WithMessage("End time is required")
            .GreaterThan(x => x.StartTime).WithMessage("End time must be after start time");
        RuleFor(x => x.Reason)
            .MaximumLength(500).WithMessage("Reason cannot exceed 500 characters");
    }
}