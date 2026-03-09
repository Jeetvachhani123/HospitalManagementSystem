using HospitalMS.Models.Entities;

namespace HospitalMS.BL.Interfaces.Repositories;

public interface IAppointmentStatusHistoryRepository
{
    Task<AppointmentStatusHistory?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<IEnumerable<AppointmentStatusHistory>> GetByAppointmentIdAsync(int appointmentId, CancellationToken cancellationToken = default);

    Task AddAsync(AppointmentStatusHistory history, CancellationToken cancellationToken = default);

    void Update(AppointmentStatusHistory history);

    void Delete(AppointmentStatusHistory history);
}