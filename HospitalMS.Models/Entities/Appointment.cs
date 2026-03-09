using HospitalMS.Models.Base;
using HospitalMS.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace HospitalMS.Models.Entities;

public class Appointment : AuditableEntity
{
    public int PatientId { get; set; }

    public int DoctorId { get; set; }

    public DateTime AppointmentDate { get; set; }

    public TimeSpan StartTime { get; set; }

    public TimeSpan EndTime { get; set; }

    public AppointmentStatus Status { get; set; } = AppointmentStatus.Scheduled;

    public AppointmentApprovalStatus ApprovalStatus { get; set; } = AppointmentApprovalStatus.Pending;

    public int? ApprovedByDoctorId { get; set; }

    public DateTime? ApprovedAt { get; set; }

    public string? RejectionReason { get; set; }

    public string? Reason { get; set; }

    public string? Notes { get; set; }

    public string? Diagnosis { get; set; }

    public string? Prescription { get; set; }

    public bool IsRescheduled { get; set; } = false;

    public int? OriginalAppointmentId { get; set; }

    public DateTime? RescheduledAt { get; set; }

    [Timestamp]
    public byte[]? RowVersion { get; set; }

    public Patient Patient { get; set; } = null!;

    public Doctor Doctor { get; set; } = null!;

    public DateTime GetFullStartDateTime() => AppointmentDate.Date + StartTime;

    public DateTime GetFullEndDateTime() => AppointmentDate.Date + EndTime;

    public TimeSpan GetDuration() => EndTime - StartTime;

    public bool IsApproved => ApprovalStatus == AppointmentApprovalStatus.Approved;

    public bool IsPending => ApprovalStatus == AppointmentApprovalStatus.Pending;

    public bool IsRejected => ApprovalStatus == AppointmentApprovalStatus.Rejected;
}