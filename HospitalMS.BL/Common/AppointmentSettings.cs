namespace HospitalMS.BL.Common;

public class AppointmentSettings
{
    public int BufferTimeMinutes { get; set; } = 15;

    public int MinAppointmentDurationMinutes { get; set; } = 30;

    public int MaxAppointmentDurationMinutes { get; set; } = 120;

    public int CancellationDeadlineHours { get; set; } = 24;

    public int MaxAdvanceBookingDays { get; set; } = 90;
}