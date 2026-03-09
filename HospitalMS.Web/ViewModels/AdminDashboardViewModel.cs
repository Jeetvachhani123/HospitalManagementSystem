namespace HospitalMS.Web.ViewModels;

public class AdminDashboardViewModel
{
    public int TotalDoctors { get; set; }

    public int TotalPatients { get; set; }

    public int TotalAppointments { get; set; }

    public int AppointmentsToday { get; set; }

    public int PendingApprovals { get; set; }

    public decimal CompletionRate { get; set; }

    public decimal NoShowRate { get; set; }

    public List<AppointmentViewModel> RecentAppointments { get; set; } = new();
}