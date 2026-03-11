using HospitalMS.BL.Interfaces.Repositories;
using HospitalMS.BL.Interfaces.Services;

using HospitalMS.Models.Enums;

namespace HospitalMS.BL.Services;

public class SearchService : ISearchService
{
    private readonly IAppointmentRepository _appointmentRepository;
    private readonly IDoctorRepository _doctorRepository;
    private readonly IPatientRepository _patientRepository;
    public SearchService(IAppointmentRepository appointmentRepository, IDoctorRepository doctorRepository, IPatientRepository patientRepository)
    {
        _appointmentRepository = appointmentRepository;
        _doctorRepository = doctorRepository;
        _patientRepository = patientRepository;
    }

    // search appointments
    public async Task<(IEnumerable<AppointmentSearchResultDto> Items, int TotalCount)> SearchAppointmentsAsync(string query, int? doctorId, int? patientId, DateTime? fromDate, DateTime? toDate, AppointmentStatus? status, int page, int pageSize)
    {
        var result = await _appointmentRepository.SearchAsync(query, doctorId, patientId, fromDate, toDate, status, page, pageSize);
        var dtos = result.Items.Select(a => new AppointmentSearchResultDto { Id = a.Id, PatientName = $"{a.Patient?.User?.FirstName} {a.Patient?.User?.LastName}", DoctorName = $"{a.Doctor?.User?.FirstName} {a.Doctor?.User?.LastName}", AppointmentDate = a.AppointmentDate, StartTime = a.StartTime, EndTime = a.EndTime, Status = a.Status.ToString(), Reason = a.Reason ?? string.Empty, Specialization = a.Doctor?.Specialization ?? "Unknown", CreatedBy = a.CreatedBy, UpdatedBy = a.UpdatedBy });
        return (dtos, result.TotalCount);
    }

    // search doctors
    public async Task<List<DoctorSearchResultDto>> SearchDoctorsAsync(string query)
    {
        if (string.IsNullOrWhiteSpace(query))
            return new List<DoctorSearchResultDto>();
        var doctors = await _doctorRepository.GetAllAsync();
        var lowerQuery = query.ToLower();
        var results = doctors.Where(d =>
            d.User.FirstName.ToLower().Contains(lowerQuery) ||
            d.User.LastName.ToLower().Contains(lowerQuery) ||
            d.Specialization.ToLower().Contains(lowerQuery) ||
            d.LicenseNumber.ToLower().Contains(lowerQuery)
        );
        return results.Select(d => new DoctorSearchResultDto
        {
            Id = d.Id,
            Name = $"{d.User.FirstName} {d.User.LastName}",
            Specialization = d.Specialization,
            LicenseNumber = d.LicenseNumber
        }).ToList();
    }

    // search patients
    public async Task<List<PatientSearchResultDto>> SearchPatientsAsync(string query)
    {
        if (string.IsNullOrWhiteSpace(query))
            return new List<PatientSearchResultDto>();
        var patients = await _patientRepository.GetAllAsync();
        var lowerQuery = query.ToLower();
        var results = patients.Where(p =>
            p.User.FirstName.ToLower().Contains(lowerQuery) ||
            p.User.LastName.ToLower().Contains(lowerQuery) ||
            p.User.Email.ToLower().Contains(lowerQuery) ||
            (p.User.PhoneNumber != null && p.User.PhoneNumber.Contains(query))
        );
        return results.Select(p => new PatientSearchResultDto
        {
            Id = p.Id,
            Name = $"{p.User.FirstName} {p.User.LastName}",
            Email = p.User.Email,
            PhoneNumber = p.User.PhoneNumber ?? string.Empty
        }).ToList();
    }
}