namespace HospitalMS.BL.DTOs.Doctor;

public class WorkingHoursDto
{
    public int Id { get; set; }

    public int DoctorId { get; set; }

    public int DayOfWeek { get; set; }

    public bool IsWorkingDay { get; set; }

    public TimeSpan StartTime { get; set; }

    public TimeSpan EndTime { get; set; }
}