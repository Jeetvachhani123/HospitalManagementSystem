namespace HospitalMS.BL.Common;

public class AppointmentSettings
{
    // buffer time in minutes
    public int BufferTimeMinutes { get; set; } = 15;

    // min duration in minutes
    public int MinAppointmentDurationMinutes { get; set; } = 30;

    // max duration in minutes
    public int MaxAppointmentDurationMinutes { get; set; } = 120;

    // cancellation deadline hours
    public int CancellationDeadlineHours { get; set; } = 24;

    // max advance booking days
    public int MaxAdvanceBookingDays { get; set; } = 90;
}