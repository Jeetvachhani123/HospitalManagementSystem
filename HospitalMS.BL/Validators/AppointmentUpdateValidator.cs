using FluentValidation;
using HospitalMS.BL.DTOs.Appointment;
using HospitalMS.BL.Interfaces.Services;
using System.Text.RegularExpressions;

namespace HospitalMS.BL.Validators;

public class AppointmentUpdateValidator : AbstractValidator<AppointmentUpdateDto>
{
    private readonly IAppointmentService? _appointmentService;
    private static readonly TimeSpan WorkingHoursStart = new TimeSpan(8, 0, 0);
    private static readonly TimeSpan WorkingHoursEnd = new TimeSpan(20, 0, 0);
    private static readonly TimeSpan MinimumDuration = new TimeSpan(0, 15, 0);
    private static readonly TimeSpan MaximumDuration = new TimeSpan(2, 0, 0);
    public AppointmentUpdateValidator()
    {
        SetupRules();
    }

    public AppointmentUpdateValidator(IAppointmentService appointmentService)
    {
        _appointmentService = appointmentService;
        SetupRules();
    }

    private void SetupRules()
    {
        RuleFor(x => x.AppointmentDate)
            .GreaterThanOrEqualTo(DateTime.Today).WithMessage("Appointment date must be today or in the future")
            .LessThanOrEqualTo(DateTime.Today.AddMonths(6)).WithMessage("Appointments can only be booked up to 6 months in advance")
            .When(x => x.AppointmentDate.HasValue);
       
        RuleFor(x => x.StartTime)
            .Must(BeWithinWorkingHours).WithMessage($"Start time must be between {WorkingHoursStart:hh\\:mm} and {WorkingHoursEnd:hh\\:mm}")
            .When(x => x.StartTime.HasValue);
       
        RuleFor(x => x.EndTime)
            .Must(BeWithinWorkingHours).WithMessage($"End time must be between {WorkingHoursStart:hh\\:mm} and {WorkingHoursEnd:hh\\:mm}")
            .When(x => x.EndTime.HasValue);
       
        RuleFor(x => x)
            .Must(HaveValidDuration).WithMessage($"Appointment duration must be between {MinimumDuration.TotalMinutes} minutes and {MaximumDuration.TotalHours} hours")
            .When(x => x.StartTime.HasValue && x.EndTime.HasValue);
        
        RuleFor(x => x.Reason)
            .MaximumLength(500).WithMessage("Reason cannot exceed 500 characters")
            .Must(BeSafeText).WithMessage("Reason contains potentially dangerous content")
            .When(x => !string.IsNullOrWhiteSpace(x.Reason));
       
        RuleFor(x => x.Notes)
            .MaximumLength(1000).WithMessage("Notes cannot exceed 1000 characters")
            .Must(BeSafeMedicalText).WithMessage("Notes contain potentially dangerous content")
            .When(x => !string.IsNullOrWhiteSpace(x.Notes));
       
        RuleFor(x => x.Diagnosis)
            .MaximumLength(2000).WithMessage("Diagnosis cannot exceed 2000 characters")
            .Must(BeSafeMedicalText).WithMessage("Diagnosis contains potentially dangerous content")
            .Must(ContainValidMedicalTerms).WithMessage("Diagnosis should contain valid medical information")
            .When(x => !string.IsNullOrWhiteSpace(x.Diagnosis));
       
        RuleFor(x => x.Prescription)
            .MaximumLength(2000).WithMessage("Prescription cannot exceed 2000 characters")
            .Must(BeSafeMedicalText).WithMessage("Prescription contains potentially dangerous content")
            .Must(ContainValidPrescriptionFormat).WithMessage("Prescription should follow proper medical format")
            .When(x => !string.IsNullOrWhiteSpace(x.Prescription));
    }

    // check within working hours
    private bool BeWithinWorkingHours(TimeSpan? time)
    {
        if (!time.HasValue)
            return true;
       
        return time.Value >= WorkingHoursStart && time.Value <= WorkingHoursEnd;
    }

    // check valid appointment duration
    private bool HaveValidDuration(AppointmentUpdateDto dto)
    {
        if (!dto.StartTime.HasValue || !dto.EndTime.HasValue)
            return true;
        
        var duration = dto.EndTime.Value - dto.StartTime.Value;
        return duration >= MinimumDuration && duration <= MaximumDuration;
    }

    // check safe text
    private bool BeSafeText(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return true;
        
        var dangerousPatterns = new[] { "<script", "javascript:", "onerror=", "onclick=", "--", ";--", "/*", "*/" };
        return !dangerousPatterns.Any(pattern => text.Contains(pattern, StringComparison.OrdinalIgnoreCase));
    }

    // check safe medical text
    private bool BeSafeMedicalText(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return true;
        
        var dangerousPatterns = new[] { "<script", "javascript:", "onerror=", "onclick=" };
        return !dangerousPatterns.Any(pattern => text.Contains(pattern, StringComparison.OrdinalIgnoreCase));
    }

    // check valid medical terms
    private bool ContainValidMedicalTerms(string? diagnosis)
    {
        if (string.IsNullOrWhiteSpace(diagnosis))
            return true;
        
        var hasLetters = Regex.IsMatch(diagnosis, @"[a-zA-Z]{3,}");
        var notTooManySpecialChars = !Regex.IsMatch(diagnosis, @"[^a-zA-Z0-9\s,.\-\(\)]{10,}");
        return hasLetters && notTooManySpecialChars;
    }

    // check valid prescription format
    private bool ContainValidPrescriptionFormat(string? prescription)
    {
        if (string.IsNullOrWhiteSpace(prescription))
            return true;
       
        var hasLetters = Regex.IsMatch(prescription, @"[a-zA-Z]{3,}");
        var notTooManySpecialChars = !Regex.IsMatch(prescription, @"[^a-zA-Z0-9\s,.\-\(\):/]{10,}");
        return hasLetters && notTooManySpecialChars;
    }
}