using HospitalMS.Models.Entities;

namespace HospitalMS.BL.Interfaces.Repositories;

public interface IDoctorWorkingHoursRepository
{
    Task<DoctorWorkingHours?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<IEnumerable<DoctorWorkingHours>> GetByDoctorIdAsync(int doctorId, CancellationToken cancellationToken = default);

    Task<DoctorWorkingHours?> GetByDoctorIdAndDayAsync(int doctorId, int dayOfWeek, CancellationToken cancellationToken = default);

    Task<DoctorWorkingHours> AddAsync(DoctorWorkingHours workingHours, CancellationToken cancellationToken = default);

    void Update(DoctorWorkingHours workingHours);

    void Delete(DoctorWorkingHours workingHours);
}