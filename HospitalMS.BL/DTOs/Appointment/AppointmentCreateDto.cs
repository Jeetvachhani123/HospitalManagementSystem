using HospitalMS.Models.Enums;

namespace HospitalMS.BL.DTOs.Appointment;

public class AppointmentCreateDto
{
    public int PatientId { get; set; }

    public int DoctorId { get; set; }

    public DateTime AppointmentDate { get; set; }

    public TimeSpan StartTime { get; set; }

    public TimeSpan EndTime { get; set; }

    public string? Reason { get; set; }
}