namespace HospitalMS.BL.DTOs.Reports;

public class RecentAppointmentSummaryDto
{
    public int Id { get; set; }
    public string PatientName { get; set; } = string.Empty;
    public string DoctorName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime AppointmentDate { get; set; }
}

public class AdminDashboardApiDto
{
    public int TotalDoctors { get; set; }
    public int TotalPatients { get; set; }
    public int TotalAppointments { get; set; }
    public int AppointmentsToday { get; set; }
    public int PendingApprovals { get; set; }
    public decimal CompletionRate { get; set; }
    public decimal NoShowRate { get; set; }
    public List<RecentAppointmentSummaryDto> RecentAppointments { get; set; } = new();
}

public class DoctorDashboardApiDto
{
    public int DoctorId { get; set; }
    public string DoctorName { get; set; } = string.Empty;
    public string Specialization { get; set; } = string.Empty;
    public bool IsAvailable { get; set; }
    public int TodayAppointmentsCount { get; set; }
    public int PendingApprovalsCount { get; set; }
    public int TotalAppointments { get; set; }
    public int CompletedAppointments { get; set; }
    public decimal ApprovalRate { get; set; }
    public int PatientsServed { get; set; }
    public List<RecentAppointmentSummaryDto> UpcomingAppointments { get; set; } = new();
}

public class PatientDashboardApiDto
{
    public int PatientId { get; set; }
    public string PatientName { get; set; } = string.Empty;
    public int TotalAppointments { get; set; }
    public int UpcomingAppointmentsCount { get; set; }
    public int CompletedAppointmentsCount { get; set; }
    public int CancelledAppointmentsCount { get; set; }
    public int PendingApprovalsCount { get; set; }
    public List<RecentAppointmentSummaryDto> RecentAppointments { get; set; } = new();
}

public class DoctorSummaryDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Specialization { get; set; } = string.Empty;
    public int YearsOfExperience { get; set; }
    public string Phone { get; set; } = string.Empty;
}

public class PatientSummaryDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string BloodGroup { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }
}

public class FullReportApiDto
{
    public AppointmentReportDto Stats { get; set; } = new();
    public List<DoctorSummaryDto> Doctors { get; set; } = new();
    public List<PatientSummaryDto> Patients { get; set; } = new();
    public List<RecentAppointmentSummaryDto> TodayAppointments { get; set; } = new();
    public DateTime GeneratedAt { get; set; }
}

public class DashboardStatsDto
{
    public int TotalDoctors { get; set; }
    public int TotalPatients { get; set; }
    public int TotalAppointments { get; set; }
    public int AppointmentsToday { get; set; }
    public int UpcomingAppointments { get; set; }
    public int CompletedAppointments { get; set; }
    public int PendingApprovals { get; set; }
    public decimal CompletionRate { get; set; }
    public decimal NoShowRate { get; set; }
    public decimal ApprovalRate { get; set; }
    public int PatientsServed { get; set; }
}

public class AppointmentReportDto
{
    public int TotalAppointments { get; set; }
    public int CompletedCount { get; set; }
    public int ScheduledCount { get; set; }
    public int CancelledCount { get; set; }
    public int NoShowCount { get; set; }
    public decimal CompletionRate { get; set; }
    public decimal NoShowRate { get; set; }
    public decimal CancellationRate { get; set; }
    public List<DoctorStatsDto> DoctorStats { get; set; } = new();
}

public class DoctorPerformanceReportDto
{
    public int DoctorId { get; set; }
    public string DoctorName { get; set; } = string.Empty;
    public int TotalAppointments { get; set; }
    public int CompletedAppointments { get; set; }
    public decimal ApprovalRate { get; set; }
    public int AverageConsultationMinutes { get; set; }
    public int PatientsServed { get; set; }
}

public class DoctorStatsDto
{
    public int DoctorId { get; set; }
    public string DoctorName { get; set; } = string.Empty;
    public string Specialization { get; set; } = string.Empty;
    public int TotalAppointments { get; set; }
    public int CompletedAppointments { get; set; }
    public decimal ApprovalRate { get; set; }
}

public class SystemStatisticsDto
{
    public int TotalDoctors { get; set; }
    public int TotalPatients { get; set; }
    public int TotalAppointments { get; set; }
    public int AppointmentsToday { get; set; }
    public int PendingApprovals { get; set; }
    public decimal CompletionRate { get; set; }
    public decimal NoShowRate { get; set; }
}

public class MonthlyTrendDto
{
    public List<MonthDataDto> Months { get; set; } = new();
}

public class MonthDataDto
{
    public string Month { get; set; } = string.Empty;
    public int TotalAppointments { get; set; }
    public int CompletedAppointments { get; set; }
    public int CancelledAppointments { get; set; }
}
