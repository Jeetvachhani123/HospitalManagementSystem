using HospitalMS.Models.Enums;

namespace HospitalMS.BL.DTOs.Appointment;

public class AppointmentResponseDto
{
    public int Id { get; set; }

    public int PatientId { get; set; }

    public string PatientName { get; set; } = string.Empty;

    public string? PatientEmail { get; set; }

    public int DoctorId { get; set; }

    public string DoctorName { get; set; } = string.Empty;

    public string DoctorSpecialization { get; set; } = string.Empty;

    public DateTime AppointmentDate { get; set; }

    public TimeSpan StartTime { get; set; }

    public TimeSpan EndTime { get; set; }

    public string Status { get; set; } = string.Empty;

    public AppointmentStatus StatusEnum { get; set; }

    public string? Reason { get; set; }

    public string? Notes { get; set; }

    public string? Diagnosis { get; set; }

    public string? Prescription { get; set; }

    public DateTime CreatedAt { get; set; }

    public string ApprovalStatus { get; set; } = string.Empty;

    public AppointmentApprovalStatus ApprovalStatusEnum { get; set; }
}