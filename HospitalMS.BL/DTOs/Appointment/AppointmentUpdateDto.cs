namespace HospitalMS.BL.DTOs.Appointment;

public class AppointmentUpdateDto
{
    public DateTime? AppointmentDate { get; set; }

    public TimeSpan? StartTime { get; set; }

    public TimeSpan? EndTime { get; set; }

    public string? Reason { get; set; }

    public string? Notes { get; set; }

    public string? Diagnosis { get; set; }

    public string? Prescription { get; set; }

    public byte[]? RowVersion { get; set; }
}