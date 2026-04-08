using FluentValidation;
using HospitalMS.BL.DTOs.Appointment;
using HospitalMS.BL.Interfaces.Services;
using System.Text.RegularExpressions;

namespace HospitalMS.BL.Validators;

public class AppointmentCreateValidatorEnhanced : AbstractValidator<AppointmentCreateDto>
{
    private readonly IAppointmentService? _appointmentService;
    private static readonly TimeSpan WorkingHoursStart = new TimeSpan(8, 0, 0);
    private static readonly TimeSpan WorkingHoursEnd = new TimeSpan(20, 0, 0);
    private static readonly TimeSpan MinimumDuration = new TimeSpan(0, 15, 0);
    private static readonly TimeSpan MaximumDuration = new TimeSpan(2, 0, 0);
    public AppointmentCreateValidatorEnhanced()
    {
        SetupRules();
    }

    public AppointmentCreateValidatorEnhanced(IAppointmentService appointmentService)
    {
        _appointmentService = appointmentService;
        SetupRules();
        SetupAsyncRules();
    }

    private void SetupRules()
    {
        RuleFor(x => x.PatientId)
            .GreaterThan(0).WithMessage("Patient ID is required");
        
        RuleFor(x => x.DoctorId)
            .GreaterThan(0).WithMessage("Doctor ID is required");
       
        RuleFor(x => x.AppointmentDate)
            .NotEmpty().WithMessage("Appointment date is required")
            .GreaterThanOrEqualTo(DateTime.Today).WithMessage("Appointment date must be today or in the future")
            .LessThanOrEqualTo(DateTime.Today.AddMonths(6)).WithMessage("Appointments can only be booked up to 6 months in advance");
        
        RuleFor(x => x.StartTime)
            .NotEmpty().WithMessage("Start time is required")
            .Must(BeWithinWorkingHours).WithMessage($"Start time must be between {WorkingHoursStart:hh\\:mm} and {WorkingHoursEnd:hh\\:mm}");
       
        RuleFor(x => x.EndTime)
            .NotEmpty().WithMessage("End time is required")
            .GreaterThan(x => x.StartTime).WithMessage("End time must be after start time")
            .Must((dto, endTime) => BeWithinWorkingHours(endTime)).WithMessage($"End time must be between {WorkingHoursStart:hh\\:mm} and {WorkingHoursEnd:hh\\:mm}");
       
        RuleFor(x => x)
            .Must(HaveValidDuration).WithMessage($"Appointment duration must be between {MinimumDuration.TotalMinutes} minutes and {MaximumDuration.TotalHours} hours")
            .Must(NotSpanMultipleDays).WithMessage("Appointment cannot span multiple days");
       
        RuleFor(x => x.Reason)
            .MaximumLength(500).WithMessage("Reason cannot exceed 500 characters")
            .Must(BeSafeText).WithMessage("Reason contains potentially dangerous content")
            .When(x => !string.IsNullOrWhiteSpace(x.Reason));
    }

    private void SetupAsyncRules()
    {
        if (_appointmentService != null)
        {
            RuleFor(x => x)
                .MustAsync(async (dto, cancellation) => await NotHaveConcurrentAppointment(dto))
                .WithMessage("This time slot conflicts with an existing appointment for the doctor");
        }
    }

    private bool BeWithinWorkingHours(TimeSpan time)
    {
        return time >= WorkingHoursStart && time <= WorkingHoursEnd;
    }

    private bool HaveValidDuration(AppointmentCreateDto dto)
    {
        var duration = dto.EndTime - dto.StartTime;
        return duration >= MinimumDuration && duration <= MaximumDuration;
    }

    private bool NotSpanMultipleDays(AppointmentCreateDto dto)
    {
        return dto.EndTime > dto.StartTime;
    }

    private async Task<bool> NotHaveConcurrentAppointment(AppointmentCreateDto dto)
    {
        if (_appointmentService == null)
            return true;
        try
        {
            var hasConflict = await _appointmentService.HasConflictAsync(dto.DoctorId, dto.AppointmentDate, dto.StartTime, dto.EndTime);
            return !hasConflict;
        }
        catch
        {
            return true;
        }
    }

    private bool BeSafeText(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return true;
        var dangerousPatterns = new[] { "<script", "javascript:", "onerror=", "onclick=", "--", ";--", "/*", "*/" };
        return !dangerousPatterns.Any(pattern => text.Contains(pattern, StringComparison.OrdinalIgnoreCase));
    }
}