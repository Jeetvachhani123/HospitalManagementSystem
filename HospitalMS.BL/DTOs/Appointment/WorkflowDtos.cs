namespace HospitalMS.BL.DTOs.Appointment;

public class CompleteAppointmentDto
{
    public string? Diagnosis { get; set; }

    public string? Prescription { get; set; }

    public string? Notes { get; set; }
}

public class RejectAppointmentDto
{
    public required string RejectionReason { get; set; }
}

public class RescheduleAppointmentDto
{
    public required DateTime NewDate { get; set; }

    public required TimeSpan NewStartTime { get; set; }

    public required TimeSpan NewEndTime { get; set; }
}

public class CancelAppointmentDto
{
    public string? CancellationReason { get; set; }
}