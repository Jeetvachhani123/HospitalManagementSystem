using HospitalMS.BL.DTOs.Reports;
using HospitalMS.DATA.Repositories;
using HospitalMS.Models.Enums;

namespace HospitalMS.BL.Services;

public interface IReportingService
{
    Task<AppointmentReportDto> GenerateAppointmentReportAsync(DateTime? startDate = null, DateTime? endDate = null);
    Task<DoctorPerformanceReportDto> GenerateDoctorPerformanceReportAsync(int doctorId);
    Task<SystemStatisticsDto> GetSystemStatisticsAsync();
    Task<MonthlyTrendDto> GetMonthlyTrendAsync(int months = 12);
}



public class ReportingService : IReportingService
{
    private readonly IAppointmentRepository _appointmentRepository;
    private readonly IDoctorRepository _doctorRepository;
    private readonly IPatientRepository _patientRepository;
    public ReportingService(IAppointmentRepository appointmentRepository, IDoctorRepository doctorRepository, IPatientRepository patientRepository)
    {
        _appointmentRepository = appointmentRepository;
        _doctorRepository = doctorRepository;
        _patientRepository = patientRepository;
    }

    public async Task<AppointmentReportDto> GenerateAppointmentReportAsync(DateTime? startDate = null, DateTime? endDate = null)
    {
        var start = startDate ?? DateTime.UtcNow.AddMonths(-1);
        var end = endDate ?? DateTime.UtcNow;
        var appointments = await _appointmentRepository.GetByDateRangeAsync(start, end);
        var appointmentList = appointments.ToList();
        var totalCount = appointmentList.Count;
        var completedCount = appointmentList.Count(a => a.Status == AppointmentStatus.Completed);
        var scheduledCount = appointmentList.Count(a => a.Status == AppointmentStatus.Scheduled);
        var cancelledCount = appointmentList.Count(a => a.Status == AppointmentStatus.Cancelled);
        var noShowCount = appointmentList.Count(a => a.Status == AppointmentStatus.NoShow);
        var report = new AppointmentReportDto
        {
            TotalAppointments = totalCount,
            CompletedCount = completedCount,
            ScheduledCount = scheduledCount,
            CancelledCount = cancelledCount,
            NoShowCount = noShowCount,
            CompletionRate = totalCount > 0 ? (decimal)completedCount / totalCount * 100 : 0,
            NoShowRate = totalCount > 0 ? (decimal)noShowCount / totalCount * 100 : 0,
            CancellationRate = totalCount > 0 ? (decimal)cancelledCount / totalCount * 100 : 0
        };

        var doctorGroups = appointmentList.GroupBy(a => a.DoctorId);
        foreach (var group in doctorGroups)
        {
            var firstAppointment = group.FirstOrDefault();
            var doctor = firstAppointment?.Doctor;
            if (doctor != null)
            {
                var doctorAppointments = group.ToList();
                report.DoctorStats.Add(new DoctorStatsDto
                {
                    DoctorId = doctor.Id,
                    DoctorName = $"{doctor.User.FirstName} {doctor.User.LastName}",
                    Specialization = doctor.Specialization,
                    TotalAppointments = doctorAppointments.Count,
                    CompletedAppointments = doctorAppointments.Count(a => a.Status == AppointmentStatus.Completed),
                    ApprovalRate = doctorAppointments.Count > 0 ? (decimal)doctorAppointments.Count(a => a.ApprovalStatus != AppointmentApprovalStatus.Rejected) / doctorAppointments.Count * 100 : 0
                });
            }
        }

        return report;
    }

    public async Task<DoctorPerformanceReportDto> GenerateDoctorPerformanceReportAsync(int doctorId)
    {
        var doctor = await _doctorRepository.GetByIdAsync(doctorId);
        if (doctor == null)
            throw new KeyNotFoundException($"Doctor with ID {doctorId} not found");

        var appointments = await _appointmentRepository.GetByDoctorIdAsync(doctorId);
        var appointmentList = appointments.ToList();
        var completedAppointments = appointmentList.Where(a => a.Status == AppointmentStatus.Completed).ToList();
        var totalAppointments = appointmentList.Count;
        var avgConsultationMinutes = 30;
        var patientsServed = appointmentList.Select(a => a.PatientId).Distinct().Count();

        return new DoctorPerformanceReportDto
        {
            DoctorId = doctorId,
            DoctorName = $"{doctor.User.FirstName} {doctor.User.LastName}",
            TotalAppointments = totalAppointments,
            CompletedAppointments = completedAppointments.Count,
            ApprovalRate = totalAppointments > 0 ? (decimal)appointmentList.Count(a => a.ApprovalStatus != AppointmentApprovalStatus.Rejected) / totalAppointments * 100 : 0,
            AverageConsultationMinutes = avgConsultationMinutes,
            PatientsServed = patientsServed
        };
    }

    public async Task<SystemStatisticsDto> GetSystemStatisticsAsync()
    {
        var totalDoctors = await _doctorRepository.CountAsync();
        var totalPatients = await _patientRepository.CountAsync();
        var today = DateTime.UtcNow.Date;
        var totalAppointments = await _appointmentRepository.CountAsync();
        var appointmentsToday = await _appointmentRepository.CountAsync(a => a.AppointmentDate >= today && a.AppointmentDate < today.AddDays(1));
        var pendingApprovals = await _appointmentRepository.CountAsync(a => a.ApprovalStatus == AppointmentApprovalStatus.Pending && a.Status == AppointmentStatus.Scheduled);
        var completedCount = await _appointmentRepository.CountAsync(a => a.Status == AppointmentStatus.Completed);
        var noShowCount = await _appointmentRepository.CountAsync(a => a.Status == AppointmentStatus.NoShow);

        return new SystemStatisticsDto
        {
            TotalDoctors = totalDoctors,
            TotalPatients = totalPatients,
            TotalAppointments = totalAppointments,
            AppointmentsToday = appointmentsToday,
            PendingApprovals = pendingApprovals,
            CompletionRate = totalAppointments > 0 ? (decimal)completedCount / totalAppointments * 100 : 0,
            NoShowRate = totalAppointments > 0 ? (decimal)noShowCount / totalAppointments * 100 : 0
        };
    }

    public async Task<MonthlyTrendDto> GetMonthlyTrendAsync(int months = 12)
    {
        var startDate = DateTime.UtcNow.AddMonths(-months);
        var data = await _appointmentRepository.GetMonthlyTrendAsync(startDate, DateTime.UtcNow);
        var monthlyData = data.Select(x => new MonthDataDto
        {
            Month = $"{x.Year}-{x.Month:D2}",
            TotalAppointments = x.Total,
            CompletedAppointments = x.Completed,
            CancelledAppointments = x.Cancelled
        }).ToList();

        return new MonthlyTrendDto
        {
            Months = monthlyData
        };
    }
}