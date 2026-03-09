namespace HospitalMS.BL.Interfaces.Services;

public interface IReportingService
{
    Task<AppointmentReportDto> GenerateAppointmentReportAsync(DateTime? startDate = null, DateTime? endDate = null);

    Task<DoctorPerformanceReportDto> GenerateDoctorPerformanceReportAsync(int doctorId);

    Task<SystemStatisticsDto> GetSystemStatisticsAsync();

    Task<MonthlyTrendDto> GetMonthlyTrendAsync(int months = 12);
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