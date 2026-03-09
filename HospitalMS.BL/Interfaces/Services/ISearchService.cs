using HospitalMS.Models.Enums;

public interface ISearchService
{
    Task<(IEnumerable<AppointmentSearchResultDto> Items, int TotalCount)> SearchAppointmentsAsync(string query, int? doctorId, int? patientId, DateTime? fromDate, DateTime? toDate, AppointmentStatus? status, int page, int pageSize);

    Task<List<DoctorSearchResultDto>> SearchDoctorsAsync(string query);

    Task<List<PatientSearchResultDto>> SearchPatientsAsync(string query);
}

public class AppointmentSearchResultDto
{
    public int Id { get; set; }

    public string PatientName { get; set; } = string.Empty;

    public string DoctorName { get; set; } = string.Empty;

    public DateTime AppointmentDate { get; set; }

    public TimeSpan StartTime { get; set; }

    public TimeSpan EndTime { get; set; }

    public string Status { get; set; } = string.Empty;

    public string Reason { get; set; } = string.Empty;

    public string Specialization { get; set; } = string.Empty;
}

public class DoctorSearchResultDto
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Specialization { get; set; } = string.Empty;

    public string LicenseNumber { get; set; } = string.Empty;
}

public class PatientSearchResultDto
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string PhoneNumber { get; set; } = string.Empty;
}