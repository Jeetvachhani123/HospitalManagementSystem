using HospitalMS.Models.Entities;

namespace HospitalMS.BL.Interfaces.Repositories;

public interface IPatientRepository
{
    Task<Patient?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<Patient?> GetByUserIdAsync(int userId, CancellationToken cancellationToken = default);

    Task<IEnumerable<Patient>> GetAllAsync(int page = 1, int pageSize = 100, CancellationToken cancellationToken = default);

    Task<Patient> AddAsync(Patient patient, CancellationToken cancellationToken = default);

    void Update(Patient patient);

    void Delete(Patient patient);

    Task<(IEnumerable<Patient> Items, int TotalCount)> SearchAsync(string? searchTerm, int page, int pageSize, CancellationToken cancellationToken = default);
}