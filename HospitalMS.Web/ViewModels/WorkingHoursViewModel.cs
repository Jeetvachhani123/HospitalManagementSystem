using System.ComponentModel.DataAnnotations;

namespace HospitalMS.Web.ViewModels;

public class WorkingHoursViewModel
{
    public int DoctorId { get; set; }

    public string DoctorName { get; set; } = string.Empty;

    public List<WorkingDayViewModel> WorkingDays { get; set; } = new();
}

public class WorkingDayViewModel
{
    public int Id { get; set; }

    public int DayOfWeek { get; set; }

    public string DayName => ((DayOfWeek)DayOfWeek).ToString();

    public bool IsWorkingDay { get; set; }

    [DataType(DataType.Time)]
    public TimeSpan StartTime { get; set; }

    [DataType(DataType.Time)]
    public TimeSpan EndTime { get; set; }
}