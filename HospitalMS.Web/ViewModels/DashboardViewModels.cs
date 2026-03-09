using System.ComponentModel.DataAnnotations;

namespace HospitalMS.Web.ViewModels;

public class PatientDashboardViewModel
{
    public PatientViewModel Patient { get; set; } = new();

    public int UpcomingAppointmentsCount { get; set; }

    public int CompletedAppointmentsCount { get; set; }

    public int CancelledAppointmentsCount { get; set; }

    public int PendingApprovalsCount { get; set; }

    public List<AppointmentViewModel> RecentAppointments { get; set; } = new();
}

public class DoctorDashboardViewModel
{
    public DoctorViewModel Doctor { get; set; } = new();

    public int TodayAppointmentsCount { get; set; }

    public int PendingApprovalsCount { get; set; }

    public int TotalPatientsTreated { get; set; }

    public List<AppointmentViewModel> UpcomingAppointments { get; set; } = new();
}