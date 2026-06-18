namespace HospitalMS.BL.DTOs.Appointment;

public sealed record AppointmentConflictRequest(
    int DoctorId,
    DateTime AppointmentDate,
    TimeSpan StartTime,
    TimeSpan EndTime,
    int? ExcludeAppointmentId = null
);
