namespace HospitalMS.Web.ViewModels;

public class CalendarViewModel
{
    public int DoctorId { get; set; }

    public string DoctorName { get; set; } = string.Empty;

    public int CurrentMonth { get; set; }

    public int CurrentYear { get; set; }

    public List<CalendarDayViewModel> Days { get; set; } = new();

    public List<UpcomingAppointmentViewModel> UpcomingAppointments { get; set; } = new();

    public int TotalAppointments { get; set; }
}

public class CalendarDayViewModel
{
    public int Day { get; set; }

    public bool IsCurrentMonth { get; set; }

    public DateTime Date { get; set; }

    public int AppointmentCount { get; set; }

    public List<UpcomingAppointmentViewModel> Appointments { get; set; } = new();

    public bool HasAppointments => AppointmentCount > 0;
}

public class UpcomingAppointmentViewModel
{
    public int Id { get; set; }

    public string PatientName { get; set; } = string.Empty;

    public DateTime AppointmentDate { get; set; }

    public TimeSpan StartTime { get; set; }

    public TimeSpan EndTime { get; set; }

    public string Status { get; set; } = string.Empty;

    public string Reason { get; set; } = string.Empty;
}