using HospitalMS.Models.Base;

namespace HospitalMS.Models.Entities;

public class DoctorWorkingHours : AuditableEntity
{
    public int DoctorId { get; set; }

    public int DayOfWeek { get; set; }

    public TimeSpan StartTime { get; set; }

    public TimeSpan EndTime { get; set; }

    public bool IsWorkingDay { get; set; } = true;

    public Doctor Doctor { get; set; } = null!;
}