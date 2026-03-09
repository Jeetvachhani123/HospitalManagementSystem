namespace HospitalMS.BL.Interfaces.Services;

public interface IExportService
{
    Task<byte[]> ExportAppointmentsToCSVAsync(List<AppointmentExportDto> appointments);

    Task<byte[]> ExportAppointmentsToExcelAsync(List<AppointmentExportDto> appointments);

    Task<byte[]> ExportAppointmentsToPdfAsync(List<AppointmentExportDto> appointments);
}

public class AppointmentExportDto
{
    public int Id { get; set; }

    public string PatientName { get; set; } = string.Empty;

    public string PatientEmail { get; set; } = string.Empty;

    public string DoctorName { get; set; } = string.Empty;

    public DateTime AppointmentDate { get; set; }

    public TimeSpan StartTime { get; set; }

    public TimeSpan EndTime { get; set; }

    public string Status { get; set; } = string.Empty;

    public string Reason { get; set; } = string.Empty;

    public string? Diagnosis { get; set; }

    public string? Prescription { get; set; }
}