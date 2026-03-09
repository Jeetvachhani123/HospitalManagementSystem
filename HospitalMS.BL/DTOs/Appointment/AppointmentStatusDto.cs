using HospitalMS.Models.Enums;

namespace HospitalMS.BL.DTOs.Appointment;

public class AppointmentStatusDto
{
    public AppointmentStatus Status { get; set; }

    public string? Notes { get; set; }

    public string? Diagnosis { get; set; }

    public string? Prescription { get; set; }

    public byte[]? RowVersion { get; set; }
}