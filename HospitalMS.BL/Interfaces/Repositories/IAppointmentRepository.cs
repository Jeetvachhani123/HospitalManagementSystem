using HospitalMS.Models.Entities;
using HospitalMS.Models.Enums;

namespace HospitalMS.BL.Interfaces.Repositories;

public interface IAppointmentRepository
{
    Task<Appointment?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<Appointment?> GetByIdForReadAsync(int id, CancellationToken cancellationToken = default);

    Task<IEnumerable<Appointment>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<IEnumerable<Appointment>> GetByPatientIdAsync(int patientId, CancellationToken cancellationToken = default);

    Task<IEnumerable<Appointment>> GetByDoctorIdAsync(int doctorId, CancellationToken cancellationToken = default);

    Task<IEnumerable<Appointment>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);

    Task<IEnumerable<Appointment>> GetByStatusAsync(AppointmentStatus status, CancellationToken cancellationToken = default);

    Task<IEnumerable<Appointment>> GetPendingApprovalsAsync(int doctorId, CancellationToken cancellationToken = default);

    Task<IEnumerable<Appointment>> GetTodaysAppointmentsAsync(CancellationToken cancellationToken = default);

    Task<bool> HasConflictAsync(int doctorId, DateTime appointmentDate, TimeSpan startTime, TimeSpan endTime, int? excludeAppointmentId = null, CancellationToken cancellationToken = default);

    Task<IEnumerable<TimeSpan>> GetAvailableSlotsAsync(int doctorId, DateTime date, int slotDurationMinutes = 30, CancellationToken cancellationToken = default);

    Task<Appointment> AddAsync(Appointment appointment, CancellationToken cancellationToken = default);

    void Update(Appointment appointment);

    void Delete(Appointment appointment);

    Task<int> CountAsync(System.Linq.Expressions.Expression<Func<Appointment, bool>>? predicate = null, CancellationToken cancellationToken = default);

    Task<IEnumerable<Appointment>> GetRecentAsync(int count, CancellationToken cancellationToken = default);

    Task<(IEnumerable<Appointment> Items, int TotalCount)> SearchAsync(string? searchTerm, int? doctorId, int? patientId, DateTime? fromDate, DateTime? toDate, AppointmentStatus? status, int page, int pageSize, CancellationToken cancellationToken = default);
}