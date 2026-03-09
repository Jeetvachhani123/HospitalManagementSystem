using HospitalMS.Models.Entities;

namespace HospitalMS.BL.Interfaces.Repositories;

public interface IDoctorRepository
{
    Task<Doctor?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<Doctor?> GetByUserIdAsync(int userId, CancellationToken cancellationToken = default);

    Task<IEnumerable<Doctor>> GetAllAsync(int page = 1, int pageSize = 100, CancellationToken cancellationToken = default);

    Task<IEnumerable<Doctor>> GetAvailableDoctorsAsync(int page = 1, int pageSize = 100, CancellationToken cancellationToken = default);

    Task<IEnumerable<Doctor>> GetBySpecializationAsync(string specialization, CancellationToken cancellationToken = default);

    Task<(IEnumerable<Doctor> Items, int TotalCount)> SearchAsync(string? searchTerm, int page, int pageSize, CancellationToken cancellationToken = default);

    Task<Doctor> AddAsync(Doctor doctor, CancellationToken cancellationToken = default);

    void Update(Doctor doctor);

    void Delete(Doctor doctor);

    Task<bool> LicenseNumberExistsAsync(string licenseNumber);
}