using HospitalMS.BL.DTOs.Appointment;
using HospitalMS.BL.DTOs.Doctor;
using HospitalMS.BL.DTOs.Patient;
using HospitalMS.Models.Enums;

namespace HospitalMS.BL.Interfaces.Services;

public interface IAppointmentService
{
    Task<AppointmentResponseDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<IEnumerable<AppointmentResponseDto>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<IEnumerable<AppointmentResponseDto>> GetByPatientIdAsync(int patientId, CancellationToken cancellationToken = default);

    Task<IEnumerable<AppointmentResponseDto>> GetByDoctorIdAsync(int doctorId, CancellationToken cancellationToken = default);

    Task<IEnumerable<AppointmentResponseDto>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);

    Task<IEnumerable<AppointmentResponseDto>> GetByStatusAsync(AppointmentStatus status, CancellationToken cancellationToken = default);

    Task<AppointmentResponseDto> CreateAsync(AppointmentCreateDto appointmentDto, CancellationToken cancellationToken = default);

    Task<AppointmentResponseDto?> UpdateAsync(int id, AppointmentUpdateDto appointmentDto, CancellationToken cancellationToken = default);

    Task<AppointmentResponseDto?> UpdateStatusAsync(int id, AppointmentStatusDto statusDto, int? currentDoctorId = null, CancellationToken cancellationToken = default);

    Task<bool> CancelAsync(int id, string? cancellationReason = null, CancellationToken cancellationToken = default);

    Task<bool> HasConflictAsync(int doctorId, DateTime appointmentDate, TimeSpan startTime, TimeSpan endTime, int? excludeAppointmentId = null, CancellationToken cancellationToken = default);

    Task<AppointmentResponseDto> ApproveAsync(int id, int doctorId, CancellationToken cancellationToken = default);

    Task<AppointmentResponseDto> RejectAsync(int id, int doctorId, string rejectionReason, CancellationToken cancellationToken = default);

    Task<IEnumerable<AppointmentResponseDto>> GetPendingApprovalsAsync(int doctorId, CancellationToken cancellationToken = default);

    Task<IEnumerable<AppointmentResponseDto>> GetTodaysAppointmentsAsync(CancellationToken cancellationToken = default);

    Task<IEnumerable<AppointmentResponseDto>> GetRecentAppointmentsAsync(int count, CancellationToken cancellationToken = default);

    Task<bool> UserHasAccessToAppointmentAsync(int userId, int appointmentId, CancellationToken cancellationToken = default);

    Task<DoctorResponseDto?> GetDoctorByUserIdAsync(int userId, CancellationToken cancellationToken = default);

    Task<PatientResponseDto?> GetPatientByUserIdAsync(int userId, CancellationToken cancellationToken = default);

    Task<int> GetAppointmentsCountAsync(int doctorId, AppointmentStatus? status = null, DateTime? date = null, CancellationToken cancellationToken = default);

    Task<(IEnumerable<AppointmentResponseDto> Items, int TotalCount)> SearchAsync(string? searchTerm, int? doctorId, int? patientId, DateTime? fromDate, DateTime? toDate, AppointmentStatus? status, int page, int pageSize, CancellationToken cancellationToken = default);

    Task<IEnumerable<AppointmentResponseDto>> GetUpcomingByDoctorIdAsync(int doctorId, int count, CancellationToken cancellationToken = default);
}